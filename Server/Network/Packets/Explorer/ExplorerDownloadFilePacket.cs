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
    [ServerPacket(Basic.ServerPackets.ExplorerDownloadFile)]
    internal class ExplorerDownloadFilePacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            var projectId = data.ReadNullableClass(data.ReadString16);
            var filePath = data.ReadString16();

            Send(client, ExplorerManager.Instance.DownloadFile(projectId, filePath));
        }

        private static void Send(NetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send((byte)ClientPackets.ExplorerDownloadFileResult, (byte)result);
        }
    }
}
