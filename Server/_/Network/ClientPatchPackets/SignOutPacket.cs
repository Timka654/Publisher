using Publisher.Basic;
using SocketCore.Utils.Buffer;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    internal class SignOutPacket
    {
        public static void Send(NetworkPatchClient client, string projectId)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.SignOut);

            packet.WriteString16(projectId);

            client.Send(packet);
        }
    }
}
