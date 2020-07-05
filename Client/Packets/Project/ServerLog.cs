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
    [ClientPacket(Basic.ClientPackets.ServerLog)]
    public class ServerLog : IPacketMessage<NetworkClient, string>
    {
        public static ServerLog Instance { get; private set; }

        public ServerLog(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent(data.ReadString16());
        }
    }
}
