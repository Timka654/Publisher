using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(PatchClientPackets.StartDownloadResult)]
    internal class StartDownloadPacket : IPacketReceive<NetworkPatchClient, (bool result, List<string>)>
    {
        protected override void Receive(InputPacketBuffer data) => Data = 
            (data.ReadBool(), data.ReadCollection(i=>i.ReadPath()).ToList());

        public async Task<(bool result, List<string>)> Send(string projectId)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.StartDownload);

            packet.WriteString16(projectId);

            return await SendWaitAsync(packet);
        }

        public StartDownloadPacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
