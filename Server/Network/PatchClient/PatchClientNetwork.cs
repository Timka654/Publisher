using Cipher.RC.RC4;
using Cipher.RSA;
using Publisher.Basic;
using Publisher.Server.Info.PacketInfo;
using Publisher.Server.Network.ClientPatchPackets;
using Publisher.Server.Info;
using SCL;
using ServerOptions.Extensions.Packet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SocketCore.Extensions.Packet;

namespace Publisher.Server.Network
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

        public PatchClientNetwork(ClientOptions<NetworkPatchClient> options) : base(options)
        {
            options.OnClientConnectEvent += Options_OnClientConnectEvent;
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
            options.OnReconnectEvent += Options_OnReconnectEvent;

            options.OnExceptionEvent += Options_OnExceptionEvent;

            options.HelperLogger = StaticInstances.ServerLogger;

            GetChangeLatestUpdateHandlePacket().OnReceiveEvent += PatchClientNetwork_OnReceiveEvent;
        }

        private void Options_OnExceptionEvent(Exception ex, NetworkPatchClient client)
        {
            StaticInstances.ServerLogger.AppendError(ex.ToString());
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
        private async void Options_OnClientConnectEvent(NetworkPatchClient client)
        {
            StaticInstances.ServerLogger.AppendInfo($"Success connected to PatchServer({Options.IpAddress}:{Options.Port})");

            SetFailed();

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

        public async Task<SignStateEnum> SignProject(ServerProjectInfo item)
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

                StaticInstances.ServerLogger.AppendError($"Project {item.Info.Name}({item.Info.Id}) cannot sign PatchServer({Options.IpAddress}:{Options.Port}) reasone ={Enum.GetName(result)} - removed");
            }
            else if (result == SignStateEnum.Ok)
                StaticInstances.ServerLogger.AppendInfo($"Project {item.Info.Name}({item.Info.Id}) success sign on PatchServer({Options.IpAddress}:{Options.Port})");

            return result;
        }

        public void SignOutProject(ServerProjectInfo item)
        {
            if (this.Options.ClientData != null)
                SignOutPacket.Send(this.Options.ClientData, item.Info.Id);

            ProjectMap.TryRemove(item.Info.Id, out var dummy);

            item.ClearPatchClient();
        }

        private AutoResetEvent downloadQueueLocker = new AutoResetEvent(true);

        internal ServerProjectInfo ProcessingProject = null;

        public async Task<bool> InitializeDownload(ServerProjectInfo item)
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

        public async Task<List<DownloadFileInfo>> GetFileList(ServerProjectInfo project)
        {
            if (ProcessingProject != project)
                throw new Exception($"{ProcessingProject} != {project}");

            return await GetFileListPacket().Send();
        }

        public async Task<(string fileName, byte[] data)[]> FinishDownload(ServerProjectInfo item)
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

        public async Task<DownloadPacketData> Download()
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

            int cnt = patchConnectionOptions.LoadPackets(typeof(PathClientPacketAttribute));

            return new PatchClientNetwork(patchConnectionOptions);
        }


        private void SetFailed()
        {
            ProcessingProject = null;
            downloadQueueLocker.Set();
        }

        public ConcurrentDictionary<string, ServerProjectInfo> ProjectMap = new ConcurrentDictionary<string, ServerProjectInfo>();
    }
}
