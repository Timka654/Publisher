using SocketCore.Utils.Buffer;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, Publisher.Basic.PatchServerPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
