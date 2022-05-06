using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, PatchServerPackets id) => packetBuffer.PacketId = (ushort)id;
    }
}
