using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Explorer
{
    [ServerPacket(Basic.ServerPackets.ExplorerCreateSignFile)]
    internal class ExplorerCreateSignFilePacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {


        }

        private static void Send(NetworkClient client, ExplorerActionResultEnum result)
        {
            client.Network.Send((byte)ClientPackets.ExplorerCreateSignFileResult, (byte)result);
        }
    }
}
