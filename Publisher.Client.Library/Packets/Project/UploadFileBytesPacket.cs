using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.PublisherClientPackets.UploadFileBytesResult)]
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

            packet.SetPacketId(Basic.PublisherServerPackets.UploadFileBytes);

            packet.WriteInt32(len);
            packet.Write(buf);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
