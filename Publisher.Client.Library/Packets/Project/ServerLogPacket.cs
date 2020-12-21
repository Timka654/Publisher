using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.ServerLog)]
    internal class ServerLogPacket : IPacketMessage<NetworkClient, string>
    {
        public static ServerLogPacket Instance { get; private set; }

        public ServerLogPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent(data.ReadString16());
        }
    }
}
