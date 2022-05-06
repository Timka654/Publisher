using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    public class NextFilePacket
    {
        public static void Send(NetworkPatchClient client, string relativePath)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.NextFile);

            packet.WritePath(relativePath);

            client.Network.Send(packet);
        }
    }
}
