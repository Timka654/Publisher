﻿using ServerPublisher.Server.Info;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NSL.Cipher.RSA;
using NSL.Cipher.RC.RC4;
using NSL.TCP.Client;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Enums;
using NSL.Logger;
using NSL.BuilderExtensions.TCPClient;
using NSL.BuilderExtensions.SocketCore;
using Microsoft.Extensions.Configuration;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using NSL.SocketCore.Utils.Logger;
using NSL.SocketClient;

namespace ServerPublisher.Server.Network
{
    class PatchClientNetwork
    {
        public string IpAddress { get; private set; }

        public int Port { get; private set; }

        private IBasicLogger? Logger;

        private TCPNetworkClient<NetworkProjectProxyClient, ClientOptions<NetworkProjectProxyClient>> client;

        public PatchClientNetwork(ProjectPatchInfo patchInfo)// : base(options)
        {
            Logger ??= new PrefixableLoggerProxy(PublisherServer.AppLogger, $"[Project Proxy({patchInfo.IpAddress}:{patchInfo.Port})]");

            IpAddress = patchInfo.IpAddress;
            Port = patchInfo.Port;

            client = TCPClientEndPointBuilder.Create()
                .WithClientProcessor<NetworkProjectProxyClient>()
                .WithOptions()
                .WithEndPoint(patchInfo.IpAddress, patchInfo.Port)
                .WithCode(builder =>
                {
                    builder.SetLogger(Logger);


                    builder.AddDefaultEventHandlers(Logger, handleOptions: DefaultEventHandlersEnum.All
#if RELEASE
                            | ~DefaultEventHandlersEnum.Receive | ~DefaultEventHandlersEnum.Send
#endif
                        , pid => Enum.GetName((PublisherPacketEnum)pid)
                        , pid => Enum.GetName((PublisherPacketEnum)pid));

                    builder.GetOptions().ConfigureRequestProcessor();

                    builder.WithInputCipher(new XRC4Cipher(patchInfo.InputCipherKey));
                    builder.WithOutputCipher(new XRC4Cipher(patchInfo.OutputCipherKey));

                    builder.WithBufferSize(PublisherServer.Configuration.GetValue<int>("patch:io:buffer_size"));

                    builder.AddConnectHandle(Options_OnClientConnectEvent);
                    builder.AddConnectHandle(Options_OnClientDisconnectEvent);
                    builder.AddExceptionHandle(Options_OnExceptionEvent);


                })
                .Build();
        }

        private RequestProcessor? requestProcessor => client.Data?.GetRequestProcessor();

        private async Task<T?> RequestAsync<T>(RequestPacketBuffer packet, Func<InputPacketBuffer, Task<T>> responseHander, bool disposeResponse = true)
        {
            T? result = default;

            var rp = requestProcessor;

            if (rp != null)
                await rp.SendRequestAsync(packet, async data =>
                {
                    if (data != null)
                        result = await responseHander(data);

                    return disposeResponse;
                });

            return result;
        }


        public async Task<ProjectProxyDownloadBytesResponseModel?> DownloadAsync(int? buffLenght = null)
        {
            buffLenght ??= GetMaxReceiveSize();

            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.DownloadBytes);

            new ProjectProxyDownloadBytesRequestModel
            {
                BufferLength = buffLenght.Value
            }
            .WriteFullTo(packet);

            var response = await RequestAsync(packet, data =>
            {
                return Task.FromResult(ProjectProxyDownloadBytesResponseModel.ReadFullFrom(data));
            });

            return response;
        }

        private static int BufferSize => PublisherServer.Configuration.GetValue<int>("patch:io:buffer_size");

        private static int GetMaxReceiveSize() => BufferSize - 32;

        private async Task ChangeLatestUpdateMessageHandle(NetworkProjectProxyClient client, InputPacketBuffer data)
        {
            //(string projectId, DateTime updateTime)
            var value = ProjectProxyUpdateDataRequestModel.ReadFullFrom(data);

            if (!ProjectMap.TryGetValue(value.ProjectId, out var proj))
                return;

            if (proj == null || (proj.Info.LatestUpdate.HasValue && proj.Info.LatestUpdate >= value.UpdateTime))
                return;

            await proj.Download(value.UpdateTime);
        }


        private async void Connect(int currentTry, bool result)
        {
            if (currentTry == int.MaxValue)
            {
#if DEBUG
                await Task.Delay(10_000);
#else
                await Task.Delay(60_000);

#endif
                await ConnectAsync();

                return;
            }

            Logger.AppendDebug($"Reconnection try: {currentTry} with result = {result}");
        }


        private void Options_OnExceptionEvent(Exception ex, NetworkProjectProxyClient client)
        {
            Logger.AppendError(ex.ToString());
        }

        private ManualResetEvent readyLocker = new ManualResetEvent(false);

        private async void Options_OnClientConnectEvent(NetworkProjectProxyClient client)
        {
            Logger.AppendInfo($"Success connected");

            foreach (var item in ProjectMap.Values.ToArray())
            {
                await SignProject(item);
            }

            readyLocker.Set();
        }

        private void Options_OnClientDisconnectEvent(NetworkProjectProxyClient client)
        {
            readyLocker.Reset();
        }

        public async Task<SignStateEnum> SignProject(ServerProjectInfo item)
        {
            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.ProjectProxySignIn);


            var userInfo = JsonSerializer.Deserialize<BasicUserInfo>(item.GetPatchSignData(), options: new JsonSerializerOptions() { IgnoreNullValues = true, IgnoreReadOnlyProperties = true, });

            RSACipher rsa = new RSACipher();
            rsa.LoadXml(userInfo.RSAPublicKey);

            var identityKey = Encoding.ASCII.GetBytes(userInfo.Id);
            identityKey = rsa.Encode(identityKey, 0, identityKey.Length);


            new ProjectProxySignInRequestModel
            {
                ProjectId = item.Info.Id,
                UserId = userInfo.Id,
                IdentityKey = identityKey,
                LatestUpdate = item.Info.LatestUpdate ?? DateTime.UtcNow

            }.WriteFullTo(packet);


            var response = await RequestAsync(packet, data =>
            {
                return Task.FromResult(ProjectProxySignInResponseModel.ReadFullFrom(data));
            });

            if (response == null || response.Result == SignStateEnum.CannotConnected)
                return SignStateEnum.CannotConnected;

            if (response.Result > SignStateEnum.Ok)
            {
                ProjectMap.TryRemove(item.Info.Id, out var dummy);

                item.ClearPatchClient();

                Logger.AppendError($"Cannot sign project {item.Info.Name}({item.Info.Id}), reasone = {Enum.GetName(response.Result)}, removed");
            }
            else
                Logger.AppendInfo($"Success sign project {item.Info.Name}({item.Info.Id})");

            return response.Result;
        }

        public void SignOutProject(ServerProjectInfo item)
        {
            var packet = OutputPacketBuffer.Create(PublisherPacketEnum.ProjectProxySignOut);

            new ProjectProxySignOutResponseModel()
            {
                ProjectId = item.Info.Id
            }.WriteFullTo(packet);

            client.Send(packet);

            ProjectMap.TryRemove(item.Info.Id, out var dummy);

            item.ClearPatchClient();
        }

        public async Task<bool> InitializeDownload(ServerProjectInfo item)
        {
            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.ProjectProxyStartDownload);

            new ProjectProxyStartDownloadRequestModel()
            {
                ProjectId = item.Info.Id,
                TransportMode = TransportModeEnum.NoArchive
            }.WriteFullTo(packet);

            var response = await RequestAsync(packet, data =>
            {
                return Task.FromResult(ProjectProxyStartDownloadResponseModel.ReadFullFrom(data));
            });

            if (response?.Result == true)
            {
                item.Info.IgnoreFilePaths = response.IgnoreFilePathes;
            }

            return response?.Result == true;
        }

        public async Task<DownloadFileInfo[]> GetFileList(ServerProjectInfo project)
        {
            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.ProjectProxyProjectFileList);

            new ProjectProxyFileListRequestModel()
            {
                ProjectId = project.Info.Id
            }.WriteFullTo(packet);

            var response = await RequestAsync(packet, data =>
            {
                return Task.FromResult(ProjectProxyProjectFileListResponseModel.ReadFullFrom(data));
            });


            return response?.FileList;
        }

        public async Task<ProjectProxyEndDownloadResponseModel> FinishDownload(ServerProjectInfo project)
        {
            var packet = RequestPacketBuffer.Create(PublisherPacketEnum.ProjectProxyFinishDownload);

            new ProjectProxyFileListRequestModel()
            {
                ProjectId = project.Info.Id
            }.WriteFullTo(packet);

            var response = await RequestAsync(packet, data =>
            {
                return Task.FromResult(ProjectProxyEndDownloadResponseModel.ReadFullFrom(data));
            });

            return response;
        }

        private ConcurrentDictionary<string, ServerProjectInfo> ProjectMap = new ConcurrentDictionary<string, ServerProjectInfo>();
    }
}
