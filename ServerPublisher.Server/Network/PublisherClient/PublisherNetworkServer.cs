using NSL.Cipher.RC.RC4;
using NSL.Logger;
using NSL.ServerOptions.Extensions;
using NSL.ServerOptions.Extensions.ConfigurationEngine;
using NSL.TCP.Server;
using ServerOptions.Extensions.Manager;
using ServerOptions.Extensions.Packet;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using ServerPublisher.Shared;
using SocketCore.Extensions.Packet;
using SocketServer;
using System;
using System.Reflection;
using Utils;

namespace ServerPublisher.Server.Network.PublisherClient
{
    internal class PublisherNetworkServer : NetworkServer<PublisherNetworkClient, PublisherNetworkServer>
    {
        protected override ILogger Logger => StaticInstances.ServerLogger;

        protected override string ServerName => "Publisher";

        protected override ServerOptions<PublisherNetworkClient> LoadConfigurationAction()
        {
            var options = StaticInstances.ServerConfiguration.LoadConfigurationServerOptions<PublisherNetworkClient>("server");

            options.inputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.input.key"));
            options.outputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.output.key"));

            return options;
        }

        protected override void LoadManagersAction()
        {
            Options.LoadManagers<PublisherNetworkClient>(typeof(ManagerLoadAttribute));
        }

        protected override void LoadReceivePacketsAction()
        {
            int len = Options.LoadPackets(typeof(ServerPacketAttribute));

            Logger.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Log, $"Loaded {ServerName} {len} packets");

            len = Options.Load<PublisherNetworkClient, ServerPacketDelegateContainerAttribute, ServerPacketAttribute>(Assembly.GetExecutingAssembly());

            Logger.Append(SocketCore.Utils.Logger.Enums.LoggerLevel.Log, $"Loaded {ServerName} {len} packet handles");
        }

        protected override void Listener_OnReceivePacket(TCPServerClient<PublisherNetworkClient> client, ushort pid, int len)
        {
#if DEBUG

            if (PacketEnumExtensions.IsDefined<PublisherServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive packet pid:{Enum.GetName((PublisherServerPackets)pid)} from {client.GetRemotePoint()}");

            if (PacketEnumExtensions.IsDefined<PatchServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive patch packet pid:{Enum.GetName((PatchServerPackets)pid)} from {client.GetRemotePoint()}");

#endif
        }

        protected override void Listener_OnSendPacket(TCPServerClient<PublisherNetworkClient> client, ushort pid, int len, string stacktrace)
        {
#if DEBUG

            if (PacketEnumExtensions.IsDefined<PublisherClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send packet pid:{Enum.GetName((PublisherClientPackets)pid)} to {client.GetRemotePoint()} (source:{stacktrace})");

            if (PacketEnumExtensions.IsDefined<PatchClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send patch packet pid:{Enum.GetName((PatchClientPackets)pid)} to {client.GetRemotePoint()} (source:{stacktrace}])");

#endif
        }

        protected override void SocketOptions_OnClientDisconnectEvent(PublisherNetworkClient client)
        {
            base.SocketOptions_OnClientDisconnectEvent(client);

            if (client != null)
                StaticInstances.SessionManager.DisconnectClient(client);
        }

        protected override void SocketOptions_OnExtensionEvent(Exception ex, PublisherNetworkClient client)
        {
            base.SocketOptions_OnExtensionEvent(ex, client);

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

            //SocketOptions_OnClientDisconnectEvent(client);
        }
    }
}
