using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System.Linq;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.PublisherServerPackets.ProjectFileList)]
    public class ProjectFileListPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.UserInfo == null || client.UserInfo.CurrentProject == null)
            {
                client.Network.Disconnect();
                return;
            }

            if (client.UserInfo.CurrentProject.ProcessUser != client.UserInfo)
            {
                if (!client.UserInfo.CurrentProject.StartProcess(client.UserInfo))
                    return;
            }

            Send(client);
        }

        private void Send(PublisherNetworkClient client)
        {
            var project = client.UserInfo.CurrentProject;

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PublisherClientPackets.FileListResult);

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
