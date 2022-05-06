using ServerPublisher.Shared;
using SocketCore;
using SocketCore.Utils.Buffer;

namespace ServerPublisher.Server.Utils
{
    public static class ClientExtensions
    {
        public static void Send(this IClient client, PublisherClientPackets packateId, byte data)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)packateId;

            packet.WriteByte(data);

            client.Send(packet);
        }
    }
}
