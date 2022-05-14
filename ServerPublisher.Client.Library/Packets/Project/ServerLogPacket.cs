using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherClientPackets.ServerLog)]
    internal class ServerLogPacket : IPacketMessage<NetworkClient, string>
    {
        public static ServerLogPacket Instance { get; private set; }

        public ServerLogPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) => InvokeEvent(data.ReadString16());
    }
}
