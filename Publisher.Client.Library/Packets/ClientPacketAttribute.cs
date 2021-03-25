using Publisher.Basic;
using ServerOptions.Extensions.Packet;

namespace Publisher.Client
{
    internal class ClientPacketAttribute : PacketAttribute
    {
        public ClientPacketAttribute(PublisherClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
