using Cipher.RC.RC4;
using Publisher.Basic;
using Publisher.Server.Network.Packets;
using SCLogger;
using ServerOptions.Extensions.ConfigurationEngine;
using ServerOptions.Extensions.Manager;
using ServerOptions.Extensions.Packet;
using SocketServer;
using System;
using System.Reflection;
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

        protected override void SocketOptions_OnClientDisconnectEvent(PublisherNetworkClient client)
        {
            base.SocketOptions_OnClientDisconnectEvent(client);

            if (client != null)
                StaticInstances.SessionManager.DisconnectClient(client);
        }
    }


//        public class NetworkEntry : NetworkServer<NetworkClient, NetworkEntry>
//    {
//        private ServerOptions<NetworkClient> options;

//        private ServerListener<NetworkClient> server;

//        public static void Start()
//        {
//            if (Instance == null)
//            {
//                StaticInstances.ServerLogger.AppendInfo("Loading server network");
//                Instance = new NetworkServer();
//                Instance.Load();
//            }
//            Instance.startNetwork();
//        }

//        public static void Stop()
//        {
//            if (Instance != null)
//            {
//                Instance.stopNetwork();
//            }
//        }

//        private void Load()
//        {
//            options = StaticInstances.ServerConfiguration.LoadConfigurationServerOptions<NetworkClient>("server");
//            options.HelperLogger = StaticInstances.ServerLogger;

//            options.LoadManagers<NetworkClient>(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));
//            options.LoadPackets(Assembly.GetExecutingAssembly(), typeof(ServerPacketAttribute));

//            options.OnClientConnectEvent += Options_OnClientConnectEvent;
//            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
//            options.OnExceptionEvent += Options_OnExceptionEvent;


//            options.inputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.input.key"));
//            options.outputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.output.key"));

//            server = new ServerListener<NetworkClient>(options);
//#if DEBUG
//            server.OnReceivePacket += Server_OnReceivePacket;
//            server.OnSendPacket += Server_OnSendPacket;
//#endif

//        }

//        private void startNetwork()
//        {
//            server.Run();
//            StaticInstances.ServerLogger.AppendInfo($"Start binding on {options.IpAddress}:{options.Port}");
//        }

//        private void stopNetwork()
//        {
//            server.Stop();
//            StaticInstances.ServerLogger.AppendInfo($"Stop binding on {options.IpAddress}:{options.Port}");
//        }

//        private void Options_OnExceptionEvent(Exception ex, NetworkClient client)
//        {
//            StaticInstances.ServerLogger.AppendInfo($"client error {client?.Network?.GetRemovePoint()} - {ex}");
//        }

//        private void Options_OnClientDisconnectEvent(NetworkClient client)
//        {
//            StaticInstances.ServerLogger.AppendInfo($"client disconnected {client?.Network?.GetRemovePoint()}");

//            if (client != null)
//                StaticInstances.SessionManager.DisconnectClient(client);
//        }

//        private void Options_OnClientConnectEvent(NetworkClient client)
//        {
//            StaticInstances.ServerLogger.AppendInfo($"client connected {client?.Network?.GetRemovePoint()}");
//        }

//        private void Server_OnSendPacket(ServerClient<NetworkClient> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
//        {
//            StaticInstances.ServerLogger.AppendDebug($"send packet pid:{(ClientPackets)pid} to {client.GetRemovePoint()} (source:{memberName}[{sourceFilePath}:{sourceLineNumber}])");
//        }

//        private void Server_OnReceivePacket(ServerClient<NetworkClient> client, ushort pid, int len)
//        {
//            StaticInstances.ServerLogger.AppendDebug($"receive packet pid:{(ClientPackets)pid} from {client.GetRemovePoint()}");
//        }
//    }
}
