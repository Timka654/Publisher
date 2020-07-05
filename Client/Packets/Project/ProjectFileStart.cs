using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.ProjectFileStartResult)]
    public class ProjectFileStart : IPacketReceive<NetworkClient,object>
    {
        private static ProjectFileStart Instance;
        public ProjectFileStart(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = null;
        }

        public static async Task Send(BasicFileInfo file)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(ServerPackets.ProjectFileStart);

            packet.WritePath(file.RelativePath);

            await Instance.SendWaitAsync(packet);
        }
    }
}
