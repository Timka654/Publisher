using NSL.SocketCore.Extensions.Packet;
using ServerPublisher.Shared;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    public class PathClientPacketAttribute : PacketAttribute
    {
        public PathClientPacketAttribute(PatchClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
