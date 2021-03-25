using Publisher.Basic;
using ServerOptions.Extensions.Packet;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    public class PathServer_ServerPacketAttribute : PacketAttribute
    {
        public PathServer_ServerPacketAttribute(Publisher.Basic.PatchClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
