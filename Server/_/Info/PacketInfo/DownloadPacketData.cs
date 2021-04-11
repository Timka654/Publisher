using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Info.PacketInfo
{
    class DownloadPacketData : IDisposable
    {
        //private InputPacketBuffer data;

        public DownloadPacketData()
        {

        }

        public DownloadPacketData(InputPacketBuffer data)
        {
            //Len = data.ReadInt32();
            Buff = data.Read(data.ReadInt32());
            EOF = data.ReadBool();
        }

        //public int Len { get; set; }

        public byte[] Buff { get; set; }

        public bool EOF { get; set; }

        public void Dispose()
        {
            Buff = null;
        }
    }
}
