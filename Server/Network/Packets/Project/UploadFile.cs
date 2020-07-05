using SocketCore.Utils.Buffer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.UploadFile)]
    public class UploadFile : IPacket<NetworkClient>
    {
        public void Receive(NetworkClient client, InputPacketBuffer data)
        {
            client.CurrentFile.IO.Write(data.Read(data.ReadInt32()));

            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.ClientPackets.UploadFileResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }
    }
}
