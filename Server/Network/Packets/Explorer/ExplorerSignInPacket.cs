using Publisher.Basic;
using SocketCore.Extensions.Buffer;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Explorer
{
    [ServerPacket(Basic.PublisherServerPackets.ExplorerSignIn)]
    internal class ExplorerSignInPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
        }

        private static void Send(PublisherNetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send(PublisherClientPackets.ExplorerSignInResult, (byte)result);
        }
    }
}
