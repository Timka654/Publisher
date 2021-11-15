using ServerOptions.Extensions.Packet;
using SocketCore.Extensions.Packet;
using System;

namespace Publisher.Server.Network.ClientPatchPackets
{
    public class PathClientPacketAttribute : PacketAttribute
    {
        public PathClientPacketAttribute(Basic.PatchClientPackets packetId) : base((ushort)packetId)
        {
        }
    }
}
