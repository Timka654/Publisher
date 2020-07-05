using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.ProjectPublishStart)]
    public class ProjectPublishStart : IPacketMessage<NetworkClient,List<string>>
    {
        public static ProjectPublishStart Instance { get; private set; }

        public ProjectPublishStart(ClientOptions<NetworkClient> options) : base(options)
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
