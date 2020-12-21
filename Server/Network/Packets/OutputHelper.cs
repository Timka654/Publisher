using Publisher.Basic;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, ClientPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
