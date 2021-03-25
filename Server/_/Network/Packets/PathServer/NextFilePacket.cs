using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System.Linq;

namespace Publisher.Server.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.NextFile)]
    public class NextFilePacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return;
            }

            client.PatchDownloadProject.NextDownloadFile(client, data.ReadPath());
        }

    }
}
