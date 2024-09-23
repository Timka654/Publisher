using NSL.SocketClient;
using NSL.SocketClient.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherPacketEnum.UploadFileBytesResult)]
    internal class UploadFileBytesPacket : IPacketReceive<NetworkClient, bool>
    {
        private static UploadFileBytesPacket Instance { get; set; }

        public UploadFileBytesPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) => Data = data.ReadBool();

        public static async Task<bool> Send(byte[] buf, int len)
        {
            var packet = new OutputPacketBuffer(len);

            packet.SetPacketId(PublisherPacketEnum.UploadFileBytes);

            packet.WriteInt32(len);
            packet.Write(buf);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
