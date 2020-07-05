using Publisher.Basic;
using Publisher.Cliient.Network.Packets;
using SCL;
using SCL.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Publisher.Client.Packets.Project
{
    [ClientPacket(Basic.ClientPackets.UploadFileResult)]
    public class UploadFile : IPacketReceive<NetworkClient, bool>
    {
        private static UploadFile Instance { get; set; }

        public UploadFile(ClientOptions<NetworkClient> options) : base(options)
        {
            Instance = this;
        }

        protected override void Receive(InputPacketBuffer data)
        {
            Data = data.ReadBool();
        }

        public static async Task<bool> Send(byte[] buf, int len)
        {
            var packet = new OutputPacketBuffer(len);

            packet.SetPacketId(Basic.ServerPackets.UploadFile);

            packet.WriteInt32(len);
            packet.Write(buf);

            return await Instance.SendWaitAsync(packet);
        }
    }
}
