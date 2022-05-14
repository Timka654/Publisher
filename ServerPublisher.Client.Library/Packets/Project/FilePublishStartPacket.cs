using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherClientPackets.FilePublishStartResult)]
    internal class FilePublishStartPacket : IPacketReceive<NetworkClient, object>
    {
        private static FilePublishStartPacket Instance;
        public FilePublishStartPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = null;
        }

        public static async Task Send(BasicFileInfo file)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherServerPackets.FilePublishStart);

            packet.WritePath(file.RelativePath);
            packet.WriteDateTime(file.FileInfo.CreationTimeUtc);
            packet.WriteDateTime(file.FileInfo.LastWriteTimeUtc);

            await Instance.SendWaitAsync(packet);
        }
    }
}
