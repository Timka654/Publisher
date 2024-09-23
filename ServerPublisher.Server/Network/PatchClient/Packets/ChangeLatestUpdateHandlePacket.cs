//using NSL.SocketClient;
//using NSL.SocketClient.Utils;
//using NSL.SocketCore.Utils.Buffer;
//using ServerPublisher.Server.Network.PublisherClient.Packets;
//using ServerPublisher.Shared.Enums;
//using System;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    [ServerPacket(PublisherPacketEnum.ChangeLatestUpdateHandle)]
//    internal class ChangeLatestUpdateHandlePacket : IPacketMessage<NetworkPatchClient,(string projectId, DateTime updateTime)>
//    {
//        protected override void Receive(InputPacketBuffer data) => 
//            InvokeEvent((data.ReadString16(), data.ReadDateTime()));
        
//        public ChangeLatestUpdateHandlePacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
//    }
//}
