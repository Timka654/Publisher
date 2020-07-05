using ClientOptions.Extensions.Packet;
using Publisher.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client
{
    public class ClientPacketAttribute : PacketAttribute
    {
        public ClientPacketAttribute(ClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
