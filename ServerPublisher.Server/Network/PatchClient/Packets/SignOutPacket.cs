using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;

namespace ServerPublisher.Server.Network.ClientPatchPackets
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
