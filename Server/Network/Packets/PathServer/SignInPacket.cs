using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;

namespace Publisher.Server.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.SignIn)]
    public class SignInPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string userId = data.ReadString16();

            string projectId = data.ReadString16();

            byte[] key = data.Read(data.ReadInt32());

            DateTime latestUpdate = data.ReadDateTime();

            StaticInstances.PatchManager.SignIn(client, projectId, userId, key, latestUpdate);
        }

        public static void Send(PublisherNetworkClient client, SignStateEnum result)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PatchClientPackets.SignInResult);
            packet.WriteByte((byte)result);

            client.Send(packet);
        }
    }

}
