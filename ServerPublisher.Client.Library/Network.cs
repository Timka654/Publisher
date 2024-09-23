using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System;
using ServerPublisher.Client.Library.Packets.Project;
using NSL.TCP.Client;
using NSL.SocketClient;
using NSL.Cipher.RC.RC4;
using ServerPublisher.Client.Library.Packets;
using NSL.ClientOptions.Extensions.Packet;
using NSL.SocketCore.Extensions.Packet;
using NSL.Utils;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library
{
    public class Network
    {
        ClientOptions<NetworkClient> options;

        internal TCPNetworkClient<NetworkClient, ClientOptions<NetworkClient>> Client { get; private set; }

        public event ProjectPublishStartPacket.OnReceiveEventHandle OnProjectPublishStartMessage
        {
            add => ProjectPublishStartPacket.Instance.OnReceiveEvent += value;
            remove => ProjectPublishStartPacket.Instance.OnReceiveEvent -= value;
        }

        public event ServerLogPacket.OnReceiveEventHandle OnServerLogMessage
        {
            add => ServerLogPacket.Instance.OnReceiveEvent += value;
            remove => ServerLogPacket.Instance.OnReceiveEvent -= value;
        }

        public Network(string ip, int port, string inputKey, string outputKey, Action<NetworkClient> disconnectedEvent, int bufferSize = 8196)
        {
            options = new ClientOptions<NetworkClient>
            {
                AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork,
                ProtocolType = System.Net.Sockets.ProtocolType.Tcp,
                IpAddress = ip,
                Port = port,
                ReceiveBufferSize = bufferSize,
                InputCipher = new XRC4Cipher(inputKey),
                OutputCipher = new XRC4Cipher(outputKey)
            };

            options.OnClientDisconnectEvent +=(e) => disconnectedEvent(e);

            options.LoadPackets(Assembly.GetExecutingAssembly(), typeof(ClientPacketAttribute));

            options.Load<NetworkClient, PacketDelegateContainerAttribute, ClientPacketAttribute>();

            Client = new TCPNetworkClient<NetworkClient, ClientOptions<NetworkClient>>(options);
        }

        public bool Connect()
        {
            return Client.Connect();
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }

        public async Task<SignStateEnum> SignIn(string projectId, BasicUserInfo user, byte[] encoded, bool compressed) => await SignInPacket.Send(projectId, user,encoded, compressed);

        public async Task<List<BasicFileInfo>> GetFileList() => await FileListPacket.Send();

        public async Task FilePublishStart(BasicFileInfo file) => await FilePublishStartPacket.Send(file);

        public async Task ProjectPublishEnd(CommandLineArgs args) => await ProjectPublishEndPacket.Send(args);

        public async Task UploadFileBytes(byte[] buf, int len) => await UploadFileBytesPacket.Send(buf,len);

    }
}
