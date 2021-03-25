using Publisher.Basic;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    internal class SignOutPacket
    {
        public static void Send(NetworkPatchClient client, string projectId)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.SignOut);

            packet.WriteString16(projectId);

            client.Send(packet);
        }
    }
}
