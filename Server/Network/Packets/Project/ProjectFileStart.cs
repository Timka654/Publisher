using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.ProjectFileStart)]
    public class ProjectFileStart : IPacket<NetworkClient>
    {
        public void Receive(NetworkClient client, InputPacketBuffer data)
        {
            byte c = data.ReadByte();

            string path = "";

            for (int i = 0; i < c; i++)
            {
                path = Path.Combine(path, data.ReadString16());
            }

            client.ProjectInfo.StartFile(client, path);

            client.Network.SendEmpty((byte)Basic.ClientPackets.ProjectFileStartResult);
        }
    }
}
