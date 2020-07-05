using Cipher.RC.RC4;
using SCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientOptions.Extensions.Packet;
using System.Reflection;

namespace Publisher.Client
{
    public class Network
    {
        ClientOptions<NetworkClient> options;
        public SocketClient<NetworkClient, ClientOptions<NetworkClient>> Client { get; private set; }

        public Network(string ip, int port, string inputKey, string outputKey)
        {
            options = new ClientOptions<NetworkClient>();
            options.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
            options.IpAddress = ip;
            options.Port = port;
            options.ReceiveBufferSize = 8196;
            options.inputCipher = new XRC4Cipher(inputKey);
            options.outputCipher = new XRC4Cipher(outputKey);

            options.LoadPackets(Assembly.GetExecutingAssembly(), typeof(ClientPacketAttribute));

            Client = new SocketClient<NetworkClient, ClientOptions<NetworkClient>>(options);
        }

        public bool Connect()
        {
            return Client.Connect();
        }

    }
}
