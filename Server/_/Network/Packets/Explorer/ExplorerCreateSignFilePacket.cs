using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Explorer
{
    [ServerPacket(Basic.PublisherServerPackets.ExplorerCreateSignFile)]
    internal class ExplorerCreateSignFilePacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {


        }

        private static void Send(PublisherNetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send((byte)PublisherClientPackets.ExplorerCreateSignFileResult, (byte)result);
        }
    }
}
