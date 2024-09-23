using NSL.SocketClient;
using NSL.SocketClient.Utils;
using ServerPublisher.Shared;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Client.Library.Packets.Project
{
    [ClientPacket(PublisherPacketEnum.FileListResult)]
    internal class FileListPacket : IPacketReceive<NetworkClient, List<BasicFileInfo>>
    {
        private static FileListPacket Instance;
        public FileListPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data) => Data = data.ReadCollection(b => new BasicFileInfo()
            {
                RelativePath = data.ReadPath(),
                Hash = data.ReadString16(),
                LastChanged = data.ReadDateTime()
            }).ToList();

        public static async Task<List<BasicFileInfo>> Send()
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherPacketEnum.PublishProjectFileList);
            return await Instance.SendWaitAsync(packet);
        }
    }

}
