using Publisher.Basic;
using Publisher.Server.Network;
using Publisher.Server.Network.Packets;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server._.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.SignOut)]
    public class SignOutPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string projectId = data.ReadString16();

            StaticInstances.PatchManager.SignOut(client, projectId);
        }
    }

}
