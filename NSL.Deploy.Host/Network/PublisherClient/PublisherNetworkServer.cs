using NSL.Cipher.RC.RC4;
using NSL.TCP.Server;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using NSL.SocketServer;
using System;
using NSL.BuilderExtensions.TCPServer;
using NSL.BuilderExtensions.SocketCore;
using ServerPublisher.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository;
using NSL.SocketCore.Utils.Logger;
using ServerPublisher.Server.Info;
using NSL.SocketCore.Utils.Exceptions;
using System.Net;
using NSL.Cipher;

namespace ServerPublisher.Server.Network.PublisherClient
{
    class PublisherNetworkServer
    {
        static TCPServerListener<PublisherNetworkClient> listener;

        static ConfigurationSettingsInfo Configuration => PublisherServer.Configuration;

        static ConfigurationSettingsInfo__Publisher__Server ServerSettings => Configuration.Publisher.Server;
        static ConfigurationSettingsInfo__Publisher__Server__IO IOSettings => ServerSettings.IO;
        static ConfigurationSettingsInfo__Publisher__Server__Cipher CipherSettings => ServerSettings.Cipher;

        static IBasicLogger Logger => PublisherServer.ServerLogger;


        static string BindingAddress => IOSettings.Address;
        static int BindingPort => IOSettings.Port;

        static int Backlog => IOSettings.Backlog;

        static string CipherInputKey => CipherSettings.InputKey;

        static string CipherOutputKey => CipherSettings.OutputKey;

        public static void Initialize()
        {
            Logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Publisher server initialize binding on {BindingAddress}:{BindingPort}");

            var addr = IPAddress.Parse(BindingAddress == "*" ? "0.0.0.0" : BindingAddress);


            var logWrapper = new NSL.Logger.PrefixableLoggerProxy(Logger, "[Publisher]");

            listener = TCPServerEndPointBuilder.Create()
                    .WithClientProcessor<PublisherNetworkClient>()
                    .WithOptions<ServerOptions<PublisherNetworkClient>>()
                    .WithCode(builder =>
                    {
                        builder.SetLogger(logWrapper);

                        var options = builder.GetOptions();

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectFileStart, ProjectPacketRepository.PublishProjectFileStartReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectFinish, ProjectPacketRepository.PublishProjectFinishReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectSignIn, ProjectPacketRepository.PublishProjectSignInReceive);

                        //options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectUploadFilePart, ProjectPacketRepository.PublishProjectUploadFilePartReceive);
                        

                        options.AddHandle((ushort)PublisherPacketEnum.PublishProjectUploadFilePart, ProjectPacketRepository.PublishProjectUploadFilePartReceive);


                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyStartFile, ProjectProxyPacketRepository.ProjectProxyStartFileReceive);
                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyDownloadBytes, ProjectProxyPacketRepository.ProjectProxyDownloadBytesReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyFinishDownload, ProjectProxyPacketRepository.ProjectProxyFinishDownloadReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxySignIn, ProjectProxyPacketRepository.ProjectProxySignInReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxySignOut, ProjectProxyPacketRepository.ProjectProxySignOutReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyStartDownload, ProjectProxyPacketRepository.ProjectProxyStartDownloadReceive);


                        builder.AddDefaultEventHandlers(logWrapper, handleOptions: DefaultEventHandlersEnum.All
#if RELEASE
                            | ~DefaultEventHandlersEnum.Receive | ~DefaultEventHandlersEnum.Send
#endif
                            , pid => Enum.GetName((PublisherPacketEnum)pid)
                            , pid => Enum.GetName((PublisherPacketEnum)pid));

                        builder.WithOutputCipher(new XorCipher(CipherOutputKey));

                        builder.WithInputCipher(new XorCipher(CipherInputKey));

                        builder.AddDisconnectHandle(client =>
                        {
                            client.ProxyClientContext?.Dispose();
                            client.PublishContext?.Dispose();
                        });

                        builder.AddExceptionHandle((ex, client) =>
                        {
                            if (ex is ConnectionLostException)
                                return;

                            var context = client?.PublishContext;

                            if (context != null)
                            {
                                context.Log(ex.ToString());

                                context.Dispose();
                            }

                            client?.Network?.Disconnect();
                        });

                        builder.GetCoreOptions().ReceiveBufferSize = 512 * 1024;
                        builder.GetCoreOptions().SegmentSize = 512;
                    })
                    .WithBindingPoint(addr, BindingPort)
                    .WithBacklog(Backlog)
                    .Build();
        }

        public static void Run()
        {
            listener.Start();
            Logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Publisher server listener started");
        }

        public static void Stop()
        {
            listener.Start();
            Logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Publisher server listener stopped");
        }
    }
}
