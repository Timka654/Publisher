using Publisher.Basic;
using ServerOptions.Extensions.Packet;
using SocketCore.Extensions.Packet;
using System;

namespace Publisher.Server.Network.PublisherClient.Packets
{
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Struct | 
        AttributeTargets.Method)]
    public class ServerPacketAttribute : PacketAttribute
    {
        public ServerPacketAttribute(PublisherServerPackets packetId) : base((ushort)packetId)
        {
        }

        public ServerPacketAttribute(PatchServerPackets packetId) : base((ushort)packetId)
        {
        }
    }

    public class ServerPacketDelegateContainerAttribute : PacketDelegateContainerAttribute { }
}
