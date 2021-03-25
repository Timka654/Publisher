using Publisher.Basic;
using Publisher.Server.Managers;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Explorer
{
    [ServerPacket(Basic.PublisherServerPackets.ExplorerDownloadFile)]
    internal class ExplorerDownloadFilePacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            var projectId = data.ReadNullableClass(data.ReadString16);
            var filePath = data.ReadString16();

            Send(client, ExplorerManager.Instance.DownloadFile(projectId, filePath));
        }

        private static void Send(PublisherNetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send((byte)PublisherClientPackets.ExplorerDownloadFileResult, (byte)result);
        }
    }
}
