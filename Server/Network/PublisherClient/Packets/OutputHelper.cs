using Publisher.Basic;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.PublisherClient.Packets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PublisherClientPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }

        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PatchClientPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
