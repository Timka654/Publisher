using Publisher.Basic;
using ServerOptions.Extensions.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets
{
    public class ServerPacketAttribute : PacketAttribute
    {
        public ServerPacketAttribute(ServerPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
