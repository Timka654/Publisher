using Publisher.Basic;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Cliient.Network.Packets
{
    public static class OutputHelper
    {
        public static void SetPacketId(this OutputPacketBuffer packetBuffer, ServerPackets id)
        {
            packetBuffer.PacketId = (ushort)id;
        }
    }
}
