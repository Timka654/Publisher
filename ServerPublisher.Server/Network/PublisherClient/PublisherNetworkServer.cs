using NSL.Cipher.RC.RC4;
using NSL.Logger.Interface;
using NSL.TCP.Server;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using NSL.SocketServer;
using System;
using NSL.BuilderExtensions.TCPServer;
using NSL.BuilderExtensions.SocketCore;
using Microsoft.Extensions.Configuration;
using ServerPublisher.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using ServerPublisher.Server.Network.PublisherClient.Packets.PacketRepository;
using NSL.SocketCore.Utils.Logger;

namespace ServerPublisher.Server.Network.PublisherClient
{
    class PublisherNetworkServer
    {
        static TCPServerListener<PublisherNetworkClient> listener;

        static IConfiguration Configuration => PublisherServer.Configuration;

        static IBasicLogger Logger => PublisherServer.AppLogger;


        static int BindingPort => Configuration.GetValue<int>("publisher:network:binding_port", 6583);

        static int Backlog => Configuration.GetValue<int>("publisher:network:backlog", 100);

        static string TransportInputCipherKey => Configuration.GetValue("publisher:network:transport:input_cipher_key", "!{b1HX11R**");

        static string TransportOutputCipherKey => Configuration.GetValue("publisher:network:transport:output_cipher_key", "!{b1HX11R**");

        public static void Initialize()
        {
            var logWrapper = new NSL.Logger.PrefixableLoggerProxy(Logger, "[Publisher]");

            listener = TCPServerEndPointBuilder.Create()
                    .WithClientProcessor<PublisherNetworkClient>()
                    .WithOptions<ServerOptions<PublisherNetworkClient>>()
                    .WithCode(builder =>
                    {
                        builder.SetLogger(logWrapper);

                        var options = builder.GetOptions();

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectFileStart, ProjectPacketRepository.PublishProjectFileStartReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectFileList, ProjectPacketRepository.PublishProjectFileListReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectFinish, ProjectPacketRepository.PublishProjectFinishReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectSignIn, ProjectPacketRepository.PublishProjectSignInReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.PublishProjectUploadFilePart, ProjectPacketRepository.PublishProjectUploadFilePartReceive);


                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyDownloadBytes, ProjectProxyPacketRepository.ProjectProxyDownloadBytesReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyFinishDownload, ProjectProxyPacketRepository.ProjectProxyFinishDownloadReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyProjectFileList, ProjectProxyPacketRepository.ProjectProxyProjectFileListReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxySignIn, ProjectProxyPacketRepository.ProjectProxySignInReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxySignOut, ProjectProxyPacketRepository.ProjectProxySignOutReceive);

                        options.AddAsyncRequestPacketHandle(PublisherPacketEnum.ProjectProxyStartDownload, ProjectProxyPacketRepository.ProjectProxyStartDownloadReceive);


                        builder.AddDefaultEventHandlers(logWrapper, handleOptions: DefaultEventHandlersEnum.All
#if RELEASE
                            | ~DefaultEventHandlersEnum.Receive | ~DefaultEventHandlersEnum.Send
#endif
                            , pid => Enum.GetName((PublisherPacketEnum)pid)
                            , pid => Enum.GetName((PublisherPacketEnum)pid));

                        builder.WithOutputCipher(new XRC4Cipher(TransportOutputCipherKey));

                        builder.WithInputCipher(new XRC4Cipher(TransportInputCipherKey));

                        builder.AddDisconnectHandle(client =>
                        {
                            PublisherServer.SessionManager.DisconnectClient(client);
                        });

                        builder.AddExceptionHandle((ex, client) =>
                        {
                            if (client != null)
                            {
                                if (client.UserInfo?.CurrentProject != null)
                                {
                                    client.UserInfo.CurrentProject.BroadcastMessage(ex.ToString());
                                }

                                if (client.Network?.GetState() == true)
                                {
                                    client.Network.Disconnect();
                                }
                            }
                        });
                    })
                    .WithBindingPoint(BindingPort)
                    .WithBacklog(Backlog)
                    .Build();

            listener.Start();
        }
    }
}
