using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PublisherPacketEnum id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
