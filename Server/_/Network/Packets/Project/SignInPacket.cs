using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.PublisherServerPackets.SignIn)]
    public class SignInPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            StaticInstances.ProjectsManager.SignIn(client,projectId, userId, key);
        }

        public static void Send(PublisherNetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }
    }
}
