//using NSL.SocketClient;
//using NSL.SocketClient.Utils;
//using ServerPublisher.Shared;
//using NSL.SocketCore.Utils.Buffer;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using ServerPublisher.Server.Network.PublisherClient.Packets;
//using ServerPublisher.Shared.Enums;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    [ServerPacket(PublisherPacketEnum.StartDownloadResult)]
//    internal class StartDownloadPacket : IPacketReceive<NetworkProjectProxyClient, (bool result, List<string>)>
//    {
//        protected override void Receive(InputPacketBuffer data) => Data = 
//            (data.ReadBool(), data.ReadCollection(i=>i.ReadPath()).ToList());

//        public async Task<(bool result, List<string>)> Send(string projectId)
//        {
//            var packet = new OutputPacketBuffer();

//            packet.SetPacketId(PublisherPacketEnum.StartDownload);

//            packet.WriteString16(projectId);

//            return await SendWaitAsync(packet);
//        }

//        public StartDownloadPacket(ClientOptions<NetworkProjectProxyClient> options) : base(options) { }
//    }
//}
