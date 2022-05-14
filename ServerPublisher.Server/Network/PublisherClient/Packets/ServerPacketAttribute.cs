using NSL.SocketCore.Extensions.Packet;
using ServerPublisher.Shared;
using System;

namespace ServerPublisher.Server.Network.PublisherClient.Packets
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
