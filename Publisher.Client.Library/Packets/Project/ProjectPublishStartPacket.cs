using Publisher.Basic;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.PublisherClientPackets.ProjectPublishStart)]
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
