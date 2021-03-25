using Publisher.Basic;
using ServerOptions.Extensions.Packet;

namespace Publisher.Server.Network.Packets
{
    public class ServerPacketAttribute : PacketAttribute
    {
        public ServerPacketAttribute(PublisherServerPackets packetId) : base((ushort)packetId)
        {
        }

        public ServerPacketAttribute(PatchServerPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
