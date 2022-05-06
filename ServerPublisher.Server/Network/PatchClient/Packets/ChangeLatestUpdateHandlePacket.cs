using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;
using System;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(PatchClientPackets.ChangeLatestUpdateHandle)]
    internal class ChangeLatestUpdateHandlePacket : IPacketMessage<NetworkPatchClient,(string projectId, DateTime updateTime)>
    {
        protected override void Receive(InputPacketBuffer data) => 
            InvokeEvent((data.ReadString16(), data.ReadDateTime()));
        
        public ChangeLatestUpdateHandlePacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
