using SocketCore.Utils.Buffer;
using System;

namespace Publisher.Server.Info.PacketInfo
{
    class DownloadPacketData : IDisposable
    {
        public DownloadPacketData()
        {

        }

        public DownloadPacketData(InputPacketBuffer data)
        {
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
