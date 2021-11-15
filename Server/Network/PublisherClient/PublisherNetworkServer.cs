using Cipher.RC.RC4;
using Publisher.Basic;
using Publisher.Server.Network.PublisherClient.Packets;
using SCLogger;
using ServerOptions.Extensions.ConfigurationEngine;
using ServerOptions.Extensions.Manager;
using ServerOptions.Extensions.Packet;
using SocketCore.Extensions.Packet;
using SocketServer;
using System;
using System.Reflection;
using Utils.Helper.Network;

namespace Publisher.Server.Network.PublisherClient
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

            options.ReceiveBufferSize = 160_000;

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

        protected override void Listener_OnReceivePacket(ServerClient<PublisherNetworkClient> client, ushort pid, int len)
        {
#if DEBUG

            if (Utils.PacketEnumExtensions.IsDefined<PublisherServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive packet pid:{Enum.GetName((PublisherServerPackets)pid)} from {client.GetRemovePoint()}");

            if (Utils.PacketEnumExtensions.IsDefined<PatchServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive patch packet pid:{Enum.GetName((PatchServerPackets)pid)} from {client.GetRemovePoint()}");

#endif
        }

        protected override void Listener_OnSendPacket(ServerClient<PublisherNetworkClient> client, ushort pid, int len, string stacktrace)
        {
#if DEBUG

            if (Utils.PacketEnumExtensions.IsDefined<PublisherClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send packet pid:{Enum.GetName((PublisherClientPackets)pid)} to {client.GetRemovePoint()} (source:{stacktrace})");

            if (Utils.PacketEnumExtensions.IsDefined<PatchClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send patch packet pid:{Enum.GetName((PatchClientPackets)pid)} to {client.GetRemovePoint()} (source:{stacktrace}])");

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
