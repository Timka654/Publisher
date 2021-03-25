using Publisher.Basic;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Linq;

namespace Publisher.Server.Network.Packets.PathServer
{
    [ServerPacket(PatchServerPackets.DownloadBytes)]
    public class DownloadBytesPacket : IPacket<PublisherNetworkClient>
    {
        public override void Receive(PublisherNetworkClient client, InputPacketBuffer data)
        {
            if (client.PatchDownloadProject == null || client.CurrentFile == null)
            {
                client.Network?.Disconnect();
                return;
            }    
            
            Send(client, data.ReadInt32());
        }

        private void Send(PublisherNetworkClient client, int bufLen)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchClientPackets.DownloadBytesResult);

            byte[] result = new byte[bufLen];

            int len = client.CurrentFile.IO.Read(result, 0, bufLen);

            packet.WriteInt32(len);

            packet.Write(result, 0, len);

            packet.WriteBool(client.CurrentFile.IO.Position == client.CurrentFile.IO.Length);

            client.Send(packet);
        }
    }
}
