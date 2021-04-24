using Cipher.RC.RC4;
using Publisher.Server.Network.Packets;
using SCLogger;
using ServerOptions.Extensions.ConfigurationEngine;
using ServerOptions.Extensions.Manager;
using ServerOptions.Extensions.Packet;
using SocketServer;
using System;
using Utils.Helper.Network;

namespace Publisher.Server.Network
{
    internal class PublisherNetworkServer : NetworkServer<PublisherNetworkClient, PublisherNetworkServer>
    {
        protected override ILogger Logger => StaticInstances.ServerLogger;

        protected override string ServerName => "Publisher";

        protected override ServerOptions<PublisherNetworkClient> LoadConfigurationAction()
        {
            var options =  StaticInstances.ServerConfiguration.LoadConfigurationServerOptions<PublisherNetworkClient>("server");

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
            Options.LoadPackets(typeof(ServerPacketAttribute));
        }
#if _DEBUG

        protected override void Listener_OnReceivePacket(ServerClient<PublisherNetworkClient> client, ushort pid, int len)
        {
            if (Utils.PacketEnumExtensions.IsDefined<PublisherServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive packet pid:{Enum.GetName((PublisherServerPackets)pid)} from {client.GetRemovePoint()}");
            if (Utils.PacketEnumExtensions.IsDefined<PatchServerPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} receive patch packet pid:{Enum.GetName((PatchServerPackets)pid)} from {client.GetRemovePoint()}");
        }

        protected override void Listener_OnSendPacket(ServerClient<PublisherNetworkClient> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            if (Utils.PacketEnumExtensions.IsDefined<PublisherClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send packet pid:{Enum.GetName((PublisherClientPackets)pid)} to {client.GetRemovePoint()} (source:{memberName}[{sourceFilePath}:{sourceLineNumber}])");

            if (Utils.PacketEnumExtensions.IsDefined<PatchClientPackets>(pid))
                StaticInstances.ServerLogger.AppendDebug($"{ServerName} send patch packet pid:{Enum.GetName((PatchClientPackets)pid)} to {client.GetRemovePoint()} (source:{memberName}[{sourceFilePath}:{sourceLineNumber}])");
        }
#endif
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
