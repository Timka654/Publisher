using Publisher.Basic;
using ServerOptions.Extensions.Packet;

namespace Publisher.Client
{
    internal class ClientPacketAttribute : PacketAttribute
    {
        public ClientPacketAttribute(ClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
