//using NSL.SocketClient;
//using NSL.SocketClient.Utils;
//using ServerPublisher.Shared;
//using NSL.SocketCore.Utils.Buffer;
//using System.Linq;
//using System.Threading.Tasks;
//using ServerPublisher.Shared.Enums;

//namespace ServerPublisher.Server.Network.ClientPatchPackets
//{
//    [ServerPacket(PublisherPacketEnum.FinishDownloadResult)]
//    internal class FinishDownloadPacket : IPacketReceive<NetworkProjectProxyClient, (string fileName, byte[] data)[]>
//    {
//        protected override void Receive(InputPacketBuffer data) => Data = data.ReadCollection<(string fileName, byte[] data)>(
//            p => (p.ReadPath(), p.Read(p.ReadInt32()))
//        ).ToArray();

//        public async Task<(string fileName, byte[] data)[]> Send()
//        {
//            var packet = new OutputPacketBuffer();

//            packet.SetPacketId(PublisherPacketEnum.FinishDownload);

//            return await SendWaitAsync(packet);
//        }

//        public FinishDownloadPacket(ClientOptions<NetworkProjectProxyClient> options) : base(options) { }
//    }
//}
