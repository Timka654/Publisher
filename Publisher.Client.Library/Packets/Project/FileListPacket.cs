using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.FileListResult)]
    internal class FileListPacket : IPacketReceive<NetworkClient, List<BasicFileInfo>>
    {
        private static FileListPacket Instance;
        public FileListPacket(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            List<BasicFileInfo> fl = new List<BasicFileInfo>();

            int len = data.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                fl.Add(new BasicFileInfo()
                {
                    RelativePath = data.ReadPath(),
                    Hash = data.ReadString16(),
                    LastChanged = data.ReadDateTime()
                });
            }


            Data = fl;
        }

        public static async Task<List<BasicFileInfo>> Send()
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(ServerPackets.ProjectFileList);
            return await Instance.SendWaitAsync(packet);
        }
    }

}
