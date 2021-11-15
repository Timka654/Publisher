using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.PublisherClientPackets.FilePublishStartResult)]
    internal class FilePublishStartPacket : IPacketReceive<NetworkClient,object>
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
