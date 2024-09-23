using NSL.SocketCore.Extensions.Packet;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets
{
    internal class ClientPacketAttribute : PacketAttribute
    {
        public ClientPacketAttribute(PublisherPacketEnum packetId) : base((ushort)packetId)
        {
        }
    }
}
