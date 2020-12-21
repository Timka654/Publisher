using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.SignIn)]
    public class SignInPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            StaticInstances.ProjectsManager.SignIn(client,projectId, userId, key);
        }

        public static void Send(NetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(ClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }
    }
}
