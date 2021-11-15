using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.PublisherClientPackets.FileListResult)]
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
            packet.SetPacketId(PublisherServerPackets.ProjectFileList);
            return await Instance.SendWaitAsync(packet);
        }
    }

}
