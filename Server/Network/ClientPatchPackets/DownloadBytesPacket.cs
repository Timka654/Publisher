using Publisher.Server.Info.PacketInfo;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Threading.Tasks;

namespace Publisher.Server.Network.ClientPatchPackets
{
    [PathServer_ServerPacket(Basic.PatchClientPackets.DownloadBytesResult)]
    internal class DownloadBytesPacket : IPacketReceive<NetworkPatchClient, DownloadPacketData>
    {
        public DownloadBytesPacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = new DownloadPacketData(data);
            //Data = new DownloadPacketData { Buff = new byte[0], EOF = false };

        }

        public async Task<DownloadPacketData> Send(int? buff = null)
        {
            if (buff == null)
                buff = StaticInstances.ServerConfiguration.GetValue<int>("patch.io.buffer.size") - sizeof(int) - sizeof(bool);

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.DownloadBytes);

            packet.WriteInt32(buff.Value);

            var result =  await SendWaitAsync(packet);

            GC.Collect(GC.GetGeneration(base.Data));

            base.Data = null;

            return result;
        }
    }
}
