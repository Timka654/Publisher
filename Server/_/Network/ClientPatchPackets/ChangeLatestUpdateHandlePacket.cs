using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    [PathServer_ServerPacket(Basic.PatchClientPackets.ChangeLatestUpdateHandle)]
    internal class ChangeLatestUpdateHandlePacket : IPacketMessage<NetworkPatchClient,(string projectId, DateTime updateTime)>
    {
        public ChangeLatestUpdateHandlePacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            InvokeEvent((data.ReadString16(), data.ReadDateTime()));
        }
    }
}
