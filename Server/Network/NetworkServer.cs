using Cipher.RC.RC4;
using Publisher.Basic;
using Publisher.Server.Network.Packets;
using ServerOptions.Extensions.ConfigurationEngine;
using ServerOptions.Extensions.Manager;
using ServerOptions.Extensions.Packet;
using SocketServer;
using System;
using System.Reflection;

namespace Publisher.Server.Network
{
    public class NetworkServer
    {
        public static NetworkServer Instance { get; private set; }

        private ServerOptions<NetworkClient> options;

        private ServerListener<NetworkClient> server;

        public static void Start()
        {
            if (Instance == null)
            {
                StaticInstances.ServerLogger.AppendInfo("Loading server network");
                Instance = new NetworkServer();
                Instance.Load();
            }
            Instance.startNetwork();
        }
        public static void Stop()
        {
            if (Instance != null)
            {
                Instance.stopNetwork();
            }
        }

        private void Load()
        {
            options = StaticInstances.ServerConfiguration.LoadConfigurationServerOptions<NetworkClient>("server");
            options.HelperLogger = StaticInstances.ServerLogger;

            options.LoadManagers<NetworkClient>(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));
            options.LoadPackets(Assembly.GetExecutingAssembly(), typeof(ServerPacketAttribute));

            options.OnClientConnectEvent += Options_OnClientConnectEvent;
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
            options.OnExceptionEvent += Options_OnExceptionEvent;


            options.inputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.input.key"));
            options.outputCipher = new XRC4Cipher(StaticInstances.ServerConfiguration.GetValue("server.io.output.key"));

            server = new ServerListener<NetworkClient>(options);
#if DEBUG
            server.OnReceivePacket += Server_OnReceivePacket;
            server.OnSendPacket += Server_OnSendPacket;
#endif

        }

        private void startNetwork()
        {
            server.Run();
            StaticInstances.ServerLogger.AppendInfo($"Start binding on {options.IpAddress}:{options.Port}");
        }

        private void stopNetwork()
        {
            server.Stop();
            StaticInstances.ServerLogger.AppendInfo($"Stop binding on {options.IpAddress}:{options.Port}");
        }

        private void Options_OnExceptionEvent(Exception ex, NetworkClient client)
        {
            StaticInstances.ServerLogger.AppendInfo($"client error {client?.Network?.GetRemovePoint()} - {ex}");
        }

        private void Options_OnClientDisconnectEvent(NetworkClient client)
        {
            StaticInstances.ServerLogger.AppendInfo($"client disconnected {client?.Network?.GetRemovePoint()}");

            if (client != null)
                StaticInstances.SessionManager.DisconnectClient(client);
        }

        private void Options_OnClientConnectEvent(NetworkClient client)
        {
            StaticInstances.ServerLogger.AppendInfo($"client connected {client?.Network?.GetRemovePoint()}");
        }

        private void Server_OnSendPacket(ServerClient<NetworkClient> client, ushort pid, int len, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            StaticInstances.ServerLogger.AppendDebug($"send packet pid:{(ClientPackets)pid} to {client.GetRemovePoint()} (source:{memberName}[{sourceFilePath}:{sourceLineNumber}])");
        }

        private void Server_OnReceivePacket(ServerClient<NetworkClient> client, ushort pid, int len)
        {
            StaticInstances.ServerLogger.AppendDebug($"receive packet pid:{(ClientPackets)pid} from {client.GetRemovePoint()}");
        }
    }
}
