using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;
using System.Linq;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(PatchClientPackets.FinishDownloadResult)]
    internal class FinishDownloadPacket : IPacketReceive<NetworkPatchClient, (string fileName, byte[] data)[]>
    {
        protected override void Receive(InputPacketBuffer data) => Data = data.ReadCollection<(string fileName, byte[] data)>(
            p => (p.ReadPath(), p.Read(p.ReadInt32()))
        ).ToArray();

        public async Task<(string fileName, byte[] data)[]> Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.FinishDownload);

            return await SendWaitAsync(packet);
        }

        public FinishDownloadPacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
