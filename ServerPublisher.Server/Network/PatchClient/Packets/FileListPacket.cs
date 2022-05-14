using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    [PathClientPacket(PatchClientPackets.ProjectFileListResult)]
    internal class FileListPacket : IPacketReceive<NetworkPatchClient, List<DownloadFileInfo>>
    {
        protected override void Receive(InputPacketBuffer data) => Data = data.ReadCollection(buf => new DownloadFileInfo()
        {
            RelativePath = data.ReadPath(),
            Hash = data.ReadString16(),
            LastChanged = data.ReadDateTime(),
            CreationTime = data.ReadDateTime(),
            ModifiedTime = data.ReadDateTime()
        }).ToList();

        public async Task<List<DownloadFileInfo>> Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchServerPackets.ProjectFileList);

            return await SendWaitAsync(packet);
        }

        public FileListPacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
