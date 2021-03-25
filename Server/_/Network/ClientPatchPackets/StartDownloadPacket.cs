using Publisher.Basic;
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
    [PathServer_ServerPacket(Basic.PatchClientPackets.StartDownloadResult)]
    internal class StartDownloadPacket : IPacketReceive<NetworkPatchClient, (bool result, List<string>)>
    {
        public StartDownloadPacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = (data.ReadBool(), data.ReadCollection(i=>i.ReadPath()).ToList());
        }

        public async Task<(bool result, List<string>)> Send(string projectId)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.StartDownload);

            packet.WriteString16(projectId);

            return await SendWaitAsync(packet);
        }
    }
}
