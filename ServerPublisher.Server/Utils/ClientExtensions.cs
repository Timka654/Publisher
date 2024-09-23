using NSL.SocketCore;
using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Server.Utils
{
    public static class ClientExtensions
    {
        public static void Send(this IClient client, PublisherPacketEnum packateId, byte data)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = (ushort)packateId;

            packet.WriteByte(data);

            client.Send(packet);
        }
    }
}
