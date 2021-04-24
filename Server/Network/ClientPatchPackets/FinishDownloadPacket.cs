using Publisher.Basic;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Linq;
using System.Threading.Tasks;

namespace Publisher.Server.Network.ClientPatchPackets
{
    [PathServer_ServerPacket(Basic.PatchClientPackets.FinishDownloadResult)]
    internal class FinishDownloadPacket : IPacketReceive<NetworkPatchClient,(string fileName, byte[] data)[]>
    {
        public FinishDownloadPacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = data.ReadCollection<(string fileName, byte[] data)>(p => { return (p.ReadPath(), p.Read(p.ReadInt32())); }).ToArray();
        }

        public async Task<(string fileName, byte[] data)[]> Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.FinishDownload);

            return await SendWaitAsync(packet);
        }
    }
}
