using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.ProjectPublishStart)]
    internal class ProjectPublishStartPacket : IPacketMessage<NetworkClient,List<string>>
    {
        public static ProjectPublishStartPacket Instance { get; private set; }

        public ProjectPublishStartPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            List<string> result = new List<string>();

            int count = data.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                result.Add(data.ReadString16());
            }

            InvokeEvent(result);
        }
    }
}
