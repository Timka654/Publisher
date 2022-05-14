using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherClientPackets.ProjectPublishStart)]
    internal class ProjectPublishStartPacket : IPacketMessage<NetworkClient, List<string>>
    {
        public static ProjectPublishStartPacket Instance { get; private set; }

        public ProjectPublishStartPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) =>
            InvokeEvent(data.ReadCollection(data => data.ReadString16()).ToList());
    }
}
