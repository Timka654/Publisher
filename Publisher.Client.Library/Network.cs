using Cipher.RC.RC4;
using SCL;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using ServerOptions.Extensions.Packet;
using Publisher.Client.Packets.Project;
using Publisher.Basic;

namespace Publisher.Client
{
    public class Network
    {
        ClientOptions<NetworkClient> options;
        internal SocketClient<NetworkClient, ClientOptions<NetworkClient>> Client { get; private set; }

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

        public Network(string ip, int port, string inputKey, string outputKey, int bufferSize = 8196)
        {
            options = new ClientOptions<NetworkClient>();
            options.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
            options.IpAddress = ip;
            options.Port = port;
            options.ReceiveBufferSize = bufferSize;
            options.inputCipher = new XRC4Cipher(inputKey);
            options.outputCipher = new XRC4Cipher(outputKey);

            options.LoadPackets(Assembly.GetExecutingAssembly(), typeof(ClientPacketAttribute));

            Client = new SocketClient<NetworkClient, ClientOptions<NetworkClient>>(options);
        }

        public bool Connect()
        {
            return Client.Connect();
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }

        public async Task<SignStateEnum> SignIn(string projectId, BasicUserInfo user, byte[] encoded) => await SignInPacket.Send(projectId, user,encoded);

        public async Task<List<BasicFileInfo>> GetFileList() => await FileListPacket.Send();

        public async Task FilePublishStart(BasicFileInfo file) => await FilePublishStartPacket.Send(file);

        public async Task ProjectPublishEnd(CommandLineArgs args) => await ProjectPublishEndPacket.Send(args);

        public async Task UploadFileBytes(byte[] buf, int len) => await UploadFileBytesPacket.Send(buf,len);

    }
}
