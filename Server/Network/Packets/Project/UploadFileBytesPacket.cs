using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.PublisherServerPackets.UploadFileBytes)]
    public class UploadFileBytesPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            client.CurrentFile.IO.Write(data.Read(data.ReadInt32()));

            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.PublisherClientPackets.UploadFileBytesResult);
            packet.WriteBool(true);

            client.Network.Send(packet);
        }
    }
}
