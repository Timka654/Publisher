﻿using Cipher.RC.RC4;
using Cipher.RSA;
using Publisher.Basic;
using Publisher.Server._.Network.ClientPatchPackets;
using Publisher.Server.Info;
using SCL;
using ServerOptions.Extensions.Packet;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher.Server._.Network
{
    class PatchClientNetwork : SocketClient<NetworkPatchClient, ClientOptions<NetworkPatchClient>>
    {
        public ClientOptions<NetworkPatchClient> Options => clientOptions;

        public SignInPacket GetSignInPacket() => ((SignInPacket)Options.Packets[(ushort)PatchClientPackets.SignInResult]);

        public StartDownloadPacket GetStartDownloadPacket() => ((StartDownloadPacket)Options.Packets[(ushort)PatchClientPackets.StartDownloadResult]);

        public FinishDownloadPacket GetFinishDownloadPacket() => ((FinishDownloadPacket)Options.Packets[(ushort)PatchClientPackets.FinishDownloadResult]);

        public DownloadBytesPacket GetDownloadBytesPacket() => ((DownloadBytesPacket)Options.Packets[(ushort)PatchClientPackets.DownloadBytesResult]);

        public ChangeLatestUpdateHandlePacket GetChangeLatestUpdateHandlePacket() => ((ChangeLatestUpdateHandlePacket)Options.Packets[(ushort)PatchClientPackets.ChangeLatestUpdateHandle]);

        public FileListPacket GetFileListPacket() => ((FileListPacket)Options.Packets[(ushort)PatchClientPackets.ProjectFileListResult]);

        //public SignInPacket GetSignInPacket() => ((SignInPacket)Options.Packets[(ushort)PatchClientPackets.SignInResult]);

        public PatchClientNetwork(ClientOptions<NetworkPatchClient> options) : base(options)
        {
            options.OnClientConnectEvent += Options_OnClientConnectEvent;
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
            options.OnReconnectEvent += Options_OnReconnectEvent;

            GetChangeLatestUpdateHandlePacket().OnReceiveEvent += PatchClientNetwork_OnReceiveEvent;
        }

        private async void Options_OnReconnectEvent(int currentTry, bool result)
        {
            if (currentTry == int.MaxValue)
            {
#if DEBUG
                await Task.Delay(10_000);
#else
                await Task.Delay(120_000);

#endif
                await ConnectAsync();
                return;
            }
                StaticInstances.ServerLogger.AppendDebug($"PatchClient {Options.IpAddress}:{Options.Port} reconnection try: {currentTry} with result = {result}");

        }

        protected override void OnReceive(ushort pid, int len)
        {
            if (Utils.PacketEnumExtensions.IsDefined<PatchClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"PatchClient receive packet pid:{Enum.GetName((PatchClientPackets)pid)} from {Options.IpAddress}:{Options.Port}");
        }

        protected override void OnSend(OutputPacketBuffer rbuff, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
        {
            if (Utils.PacketEnumExtensions.IsDefined<PatchServerPackets>(rbuff.PacketId))
                StaticInstances.ServerLogger.AppendDebug($"PatchClient send packet pid:{Enum.GetName((PatchServerPackets)rbuff.PacketId)} to {Options.IpAddress}:{Options.Port} (source:{memberName}[{sourceFilePath}:{sourceLineNumber}])");
        }

        private async void Options_OnClientConnectEvent(NetworkPatchClient client)
        {
            StaticInstances.ServerLogger.AppendInfo($"Success connected to PatchServer({Options.IpAddress}:{Options.Port})");

            foreach (var item in ProjectMap.Values.ToArray())
            {
                await SignProject(item);
            }
        }

        private void Options_OnClientDisconnectEvent(NetworkPatchClient client)
        {
            if (ProcessingProject != null)
                SetFailed();
        }

        private async void PatchClientNetwork_OnReceiveEvent((string projectId, DateTime updateTime) value)
        {
            if (!ProjectMap.TryGetValue(value.projectId, out var proj))
                return;

            if (proj == null || (proj.Info.LatestUpdate.HasValue && proj.Info.LatestUpdate > value.updateTime))
                return;

            await proj.Download(value.updateTime);
        }

        public async Task<SignStateEnum> SignProject(ProjectInfo item)
        {

            var userInfo = JsonSerializer.Deserialize<BasicUserInfo>(item.GetPatchSignData(), options: new JsonSerializerOptions() { IgnoreNullValues = true, IgnoreReadOnlyProperties = true, });

            RSACipher rsa = new RSACipher();
            rsa.LoadXml(userInfo.RSAPublicKey);

            var temp = Encoding.ASCII.GetBytes(userInfo.Id);
            temp = rsa.Encode(temp, 0, temp.Length);


            var result = await GetSignInPacket().Send(item.Info.Id, userInfo.Id, temp, item.Info.LatestUpdate);
            if (result != SignStateEnum.Ok && result != SignStateEnum.CannotConnected)
            {
                ProjectMap.TryRemove(item.Info.Id, out var dummy);

                item.ClearPatchClient();

                StaticInstances.ServerLogger.AppendError($"Project {item.Info.Name}({item.Info.Id}) cannot sign PatchServer({Options.IpAddress}:{Options.Port}) reasone ={Enum.GetName<SignStateEnum>(result)} - removed");
            }
            else if (result == SignStateEnum.Ok)
                StaticInstances.ServerLogger.AppendInfo($"Project {item.Info.Name}({item.Info.Id}) success sign on PatchServer({Options.IpAddress}:{Options.Port})");

            return result;
        }

        public void SignOutProject(ProjectInfo item)
        {
            SignOutPacket.Send(this.Options.ClientData, item.Info.Id);
        }

        private AutoResetEvent downloadQueueLocker = new AutoResetEvent(true);

        internal ProjectInfo ProcessingProject = null;

        public async Task<bool> InitializeDownload(ProjectInfo item)
        {
            downloadQueueLocker.WaitOne();

            var result = await GetStartDownloadPacket().Send(item.Info.Id);

            if (!result.result)
            {
                downloadQueueLocker.Set();
            }
            else
            {
                ProcessingProject = item;
                item.Info.IgnoreFilePaths = result.Item2;
            }

            return result.result;
        }

        public async Task<List<BasicFileInfo>> GetFileList(ProjectInfo project)
        {
            if (ProcessingProject != project)
                throw new Exception($"{ProcessingProject} != {project}");

            return await GetFileListPacket().Send();
        }

        public async Task<(string fileName, byte[] data)[]> FinishDownload(ProjectInfo item)
        {
            if (item != ProcessingProject)
                throw new Exception($"{item} != {ProcessingProject}");

            var result = await GetFinishDownloadPacket().Send();

            ProcessingProject = null;

            downloadQueueLocker.Set();

            return result;
        }

        public void NextDownloadFile(BasicFileInfo file)
        {
            NextFilePacket.Send(this.Options.ClientData, file.RelativePath);
        }

        public async Task<( byte[] buffer,bool eof)> Download()
        {
            return await GetDownloadBytesPacket().Send();
        }

        public static PatchClientNetwork Load(ProjectPatchInfo patchInfo)
        {
            var patchConnectionOptions = new ClientOptions<NetworkPatchClient>();

            patchConnectionOptions.IpAddress = patchInfo.IpAddress;
            patchConnectionOptions.Port = patchInfo.Port;
            patchConnectionOptions.inputCipher = new XRC4Cipher(patchInfo.InputCipherKey);
            patchConnectionOptions.outputCipher = new XRC4Cipher(patchInfo.OutputCipherKey);

            patchConnectionOptions.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
            
            patchConnectionOptions.EnableAutoRecovery = true;
            patchConnectionOptions.RecoveryWaitTime = 10_000;
            patchConnectionOptions.MaxRecoveryTryTime = int.MaxValue;

            patchConnectionOptions.HelperLogger = StaticInstances.ServerLogger;
            patchConnectionOptions.MaxRecoveryTryTime = int.MaxValue;
            patchConnectionOptions.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
            patchConnectionOptions.ReceiveBufferSize = StaticInstances.ServerConfiguration.GetValue<int>("patch.io.buffer.size");

            //patchConnectionOptions.OnClientConnectEvent += PatchOptions_OnClientConnectEvent;

            patchConnectionOptions.LoadPackets(typeof(PathServer_ServerPacketAttribute));

            return new PatchClientNetwork(patchConnectionOptions);
        }


        private void SetFailed()
        {
            ProcessingProject = null;
            downloadQueueLocker.Set();
        }

        public ConcurrentDictionary<string, ProjectInfo> ProjectMap = new ConcurrentDictionary<string, ProjectInfo>();
    }
}
