using Publisher.Basic;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.ClientPatchPackets
{
    public class NextFilePacket
    {
        public static void Send(NetworkPatchClient client, string relativePath)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.NextFile);

            packet.WritePath(relativePath);

            client.Network.Send(packet);
        }
    }
}
