using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System.Linq;

namespace Publisher.Server.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.ProjectFileList)]
    public class ProjectFileListPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null)
            {
                client.Network?.Disconnect();
                return;
            }    
            
            Send(client);
        }

        private void Send(PublisherNetworkClient client)
        {
            var project = client.PatchDownloadProject;

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchClientPackets.ProjectFileListResult);

            packet.WriteCollection(project.FileInfoList.Where(x => x.FileInfo.Exists), (p, item) =>
            {
                p.WritePath(item.RelativePath);
                p.WriteString16(item.Hash);
                p.WriteDateTime(item.LastChanged);
            });

            client.Send(packet);
        }
    }
}
