using Publisher.Basic;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    public class NextFilePacket
    {
        public static void Send(NetworkPatchClient client, string relativePath)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.NextFile);

            packet.WritePath(relativePath);

            client.Network.Send(packet);
        }
    }
}
