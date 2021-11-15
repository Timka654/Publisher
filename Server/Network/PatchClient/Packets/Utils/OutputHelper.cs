using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.ClientPatchPackets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, Basic.PatchServerPackets id) => packetBuffer.PacketId = (ushort)id;
    }
}
