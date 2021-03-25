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
    [PathServer_ServerPacket(Basic.PatchClientPackets.ProjectFileListResult)]
    internal class FileListPacket : IPacketReceive<NetworkPatchClient, List<BasicFileInfo>>
    {
        public FileListPacket(ClientOptions<NetworkPatchClient> options) : base(options)
        {
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

        public async Task<List<BasicFileInfo>> Send()
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(Basic.PatchServerPackets.ProjectFileList);

            return await SendWaitAsync(packet);
        }
    }
}
