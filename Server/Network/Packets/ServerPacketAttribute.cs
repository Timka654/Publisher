using Publisher.Basic;
using ServerOptions.Extensions.Packet;

namespace Publisher.Server.Network.Packets
{
    public class ServerPacketAttribute : PacketAttribute
    {
        public ServerPacketAttribute(ServerPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
