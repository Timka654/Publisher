using ServerPublisher.Server.Network.ClientPatchPackets;
using ServerPublisher.Server.Info;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NSL.Cipher.RSA;
using NSL.Cipher.RC.RC4;
using NSL.TCP.Client;
using NSL.Utils;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Enums;
using NSL.Logger;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.SocketCore;
using Microsoft.Extensions.Configuration;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;

namespace ServerPublisher.Server.Network
{
    class PatchClientNetwork
    {
        public string IpAddress { get; private set; }

        public int Port { get; private set; }

        public PatchClientNetwork(ProjectPatchInfo patchInfo)// : base(options)
        {
            IpAddress = patchInfo.IpAddress;
            Port = patchInfo.Port;

            client = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<NetworkPatchClient>()
                .WithOptions()
                .WithEndPoint(patchInfo.IpAddress, patchInfo.Port)
                .WithCode(builder =>
                {
                    builder.SetLogger(PublisherServer.AppLogger);

                    builder.GetOptions().ConfigureRequestProcessor();

                    builder.WithInputCipher(new XRC4Cipher(patchInfo.InputCipherKey));
                    builder.WithOutputCipher(new XRC4Cipher(patchInfo.OutputCipherKey));

                    builder.WithBufferSize(GetDefaultBufferSize());

                    builder.AddConnectHandle(Options_OnClientConnectEvent);
                    builder.AddConnectHandle(Options_OnClientDisconnectEvent);
                    builder.AddExceptionHandle(Options_OnExceptionEvent);
                })
                .Build();

            //options.OnClientConnectEvent += Options_OnClientConnectEvent;
            //options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
            //options.OnReconnectEvent += Options_OnReconnectEvent;

            //options.OnExceptionEvent += Options_OnExceptionEvent;

            //options.HelperLogger = PublisherServer.ServerLogger;

            //GetChangeLatestUpdateHandlePacket().OnReceiveEvent += PatchClientNetwork_OnReceiveEvent;
        }

        private RequestProcessor requestProcessor;

        public async Task<ProjectProxyDownloadBytesResponseModel> DownloadAsync(int? buffLenght = null)
        {
            buffLenght ??= GetDefaultBufferSize();

            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.DownloadBytes);

            new ProjectProxyDownloadBytesRequestModel
            {
                BufferLength = buffLenght.Value
            }
            .WriteFullTo(packet);

            //await client.Data.GetRequestProcessor().SendRequestAsync(packet)

            await requestProcessor.SendRequestAsync(packet, data =>
            {

                return Task.FromResult(true);
            });

            packet.SetPacketId(PublisherPacketEnum.DownloadBytes);

            packet.WriteInt32(buffLenght.Value);

            var result = await SendWaitAsync(packet);

            GC.Collect(GC.GetGeneration(Data));

            Data = null;

            return result;
        }

        private static int GetDefaultBufferSize() => PublisherServer.Configuration.GetValue<int>("patch:io:buffer_size") - sizeof(int) - sizeof(bool);

        private void ChangeLatestUpdateMessageHandle(NetworkPatchClient client, InputPacketBuffer data)
        {
            //(string projectId, DateTime updateTime)
        }



        private void Options_OnExceptionEvent(Exception ex, NetworkPatchClient client)
        {
            PublisherServer.ServerLogger.AppendError(ex.ToString());
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

            PublisherServer.ServerLogger.AppendDebug($"PatchClient {Options.IpAddress}:{Options.Port} reconnection try: {currentTry} with result = {result}");
        }

        protected override void OnReceive(ushort pid, int len)
        {
            if (PacketEnumExtensions.IsDefined<PublisherPacketEnum>(pid))
                PublisherServer.ServerLogger.AppendDebug($"PatchClient receive packet pid:{Enum.GetName((PublisherPacketEnum)pid)} from {Options.IpAddress}:{Options.Port}");
        }
        private async void Options_OnClientConnectEvent(NetworkPatchClient client)
        {
            PublisherServer.ServerLogger.AppendInfo($"Success connected to PatchServer({Options.IpAddress}:{Options.Port})");

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

                PublisherServer.ServerLogger.AppendError($"Project {item.Info.Name}({item.Info.Id}) cannot sign PatchServer({Options.IpAddress}:{Options.Port}) reasone ={Enum.GetName(result)} - removed");
            }
            else if (result == SignStateEnum.Ok)
                PublisherServer.ServerLogger.AppendInfo($"Project {item.Info.Name}({item.Info.Id}) success sign on PatchServer({Options.IpAddress}:{Options.Port})");

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

        private TCPClient<NetworkPatchClient> client;


        private void SetFailed()
        {
            ProcessingProject = null;
            downloadQueueLocker.Set();
        }

        public ConcurrentDictionary<string, ServerProjectInfo> ProjectMap = new ConcurrentDictionary<string, ServerProjectInfo>();
    }
}
