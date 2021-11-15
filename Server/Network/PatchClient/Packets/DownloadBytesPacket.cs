using Publisher.Server.Info.PacketInfo;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace Publisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(Basic.PatchClientPackets.DownloadBytesResult)]
    internal class DownloadBytesPacket : IPacketReceive<NetworkPatchClient, DownloadPacketData>
    {
        protected override void Receive(InputPacketBuffer data) => Data = new DownloadPacketData(data);

        public async Task<DownloadPacketData> Send(int? buffLenght = null)
        {
            buffLenght ??= GetDefaultBuffSize();

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.DownloadBytes);

            packet.WriteInt32(buffLenght.Value);

            var result = await SendWaitAsync(packet);

            GC.Collect(GC.GetGeneration(Data));
            
            Data = null;

            return result;
        }

        private static int GetDefaultBuffSize() => StaticInstances.ServerConfiguration.GetValue<int>("patch.io.buffer.size") - sizeof(int) - sizeof(bool);

        public DownloadBytesPacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
