using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets
{
    internal static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PublisherPacketEnum id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
