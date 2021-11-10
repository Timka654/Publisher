using Publisher.Basic;
using SocketCore.Extensions.Buffer;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Explorer
{
    [ServerPacket(Basic.PublisherServerPackets.ExplorerGetProjectList)]
    internal class ExplorerGetProjectListPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void Send(PublisherNetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send(PublisherClientPackets.ExplorerGetProjectListResult, (byte)result);
        }
    }
}
