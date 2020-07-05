using Publisher.Basic;
using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.ProjectFileList)]
    public class ProjectFileList : IPacket<NetworkClient>
    {
        public void Receive(NetworkClient client, InputPacketBuffer data)
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

        private void Send(NetworkClient client)
        {
            var project = client.UserInfo.CurrentProject;

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.ClientPackets.FileListResult);

            var fl = project.FileInfoList.Where(x => x.FileInfo.Exists);

            packet.WriteInt32(fl.Count());

            foreach (var item in fl.ToArray())
            {
                packet.WritePath(item.RelativePath);
                packet.WriteString16(item.Hash);
                packet.WriteDateTime(item.LastChanged);
            }

            client.Send(packet);
        }
    }
}
