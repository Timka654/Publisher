using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.ServerPackets.UploadFileBytes)]
    public class UploadFileBytesPacket : IPacket<NetworkClient>
    {
        public override void Receive(NetworkClient client, InputPacketBuffer data)
        {
            client.CurrentFile.IO.Write(data.Read(data.ReadInt32()));

            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.ClientPackets.UploadFileBytesResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }
    }
}
