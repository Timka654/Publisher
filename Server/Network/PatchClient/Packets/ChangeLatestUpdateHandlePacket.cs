using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;

namespace Publisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(Basic.PatchClientPackets.ChangeLatestUpdateHandle)]
    internal class ChangeLatestUpdateHandlePacket : IPacketMessage<NetworkPatchClient,(string projectId, DateTime updateTime)>
    {
        protected override void Receive(InputPacketBuffer data) => 
            InvokeEvent((data.ReadString16(), data.ReadDateTime()));
        
        public ChangeLatestUpdateHandlePacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
