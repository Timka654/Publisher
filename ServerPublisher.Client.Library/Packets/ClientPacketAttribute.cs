using NSL.SocketCore.Extensions.Packet;
using ServerPublisher.Shared;

namespace ServerPublisher.Client.Library.Packets
{
    internal class ClientPacketAttribute : PacketAttribute
    {
        public ClientPacketAttribute(PublisherClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
