using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System.IO;

namespace Publisher.Server.Network.Packets.Project
{
    [ServerPacket(Basic.PublisherServerPackets.FilePublishStart)]
    public class FilePublishStartPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            byte c = data.ReadByte();

            string path = "";

            for (int i = 0; i < c; i++)
            {
                path = Path.Combine(path, data.ReadString16());
            }

            client.ProjectInfo.StartFile(client, path);

            client.Network.SendEmpty((byte)Basic.PublisherClientPackets.FilePublishStartResult);
        }
    }
}
