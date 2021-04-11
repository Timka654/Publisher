using Publisher.Basic;
using Publisher.Server.Network;
using Publisher.Server.Network.Packets;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server._.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.FinishDownload)]
    public class FinishDownloadPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            StaticInstances.PatchManager.FinishDownload(client);
        }
    }

}
