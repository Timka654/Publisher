using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerPublisher.Shared.Info;
using ServerPublisher.Server.Network.PublisherClient.Packets;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Server.Network.ClientPatchPackets
{
    [ServerPacket(PublisherPacketEnum.ProjectFileListResult)]
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

            packet.SetPacketId(PublisherPacketEnum.PublishProjectFileList);

            return await SendWaitAsync(packet);
        }

        public FileListPacket(ClientOptions<NetworkPatchClient> options) : base(options) { }
    }
}
