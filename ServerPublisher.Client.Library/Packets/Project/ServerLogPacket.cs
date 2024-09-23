using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Utils.Buffer;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherPacketEnum.ServerLog)]
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
