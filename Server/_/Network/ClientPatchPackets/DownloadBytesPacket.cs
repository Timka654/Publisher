using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Network.ClientPatchPackets
{
    [PathServer_ServerPacket(Basic.PatchClientPackets.DownloadBytesResult)]
    internal class DownloadBytesPacket : IPacketReceive<NetworkPatchClient, (byte[] buff,  bool eof)>
    {
        public DownloadBytesPacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = (data.Read(data.ReadInt32()),data.ReadBool());
        }

        public async Task<(byte[] buff, bool eof)> Send(int buff = 81_920)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.DownloadBytes);

            packet.WriteInt32(buff);

            return await SendWaitAsync(packet);
        }
    }
}
