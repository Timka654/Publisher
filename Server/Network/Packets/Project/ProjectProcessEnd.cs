using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.ProjectEnd)]
    public class ProjectProcessEnd : IPacket<NetworkClient>
    {
        public void Receive(NetworkClient client, InputPacketBuffer data)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();

            int c = data.ReadByte();

            for (int i = 0; i < c; i++)
            {
                args.Add(data.ReadString16(), data.ReadString16());
            }

            client.ProjectInfo.StopProcess(client, true, args);


            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.ClientPackets.ProjectPublishEndResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }
    }
}
