using Publisher.Basic;
using Publisher.Server.Network;
using Publisher.Server.Network.Packets;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher.Server._.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.StartDownload)]
    public class StartDownloadPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            string projectId = data.ReadString16();

            StaticInstances.PatchManager.StartDownload(client, projectId);
        }

        public static void Send(PublisherNetworkClient client, bool result, List<string> ignoreFilePaths)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PatchClientPackets.StartDownloadResult);
            packet.WriteBool(result);
            packet.WriteCollection(ignoreFilePaths, (b,v) => b.WritePath(v));

            client.Send(packet);
        }
    }

}
