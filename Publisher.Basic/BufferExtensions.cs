using SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Basic
{
    public static class BufferExtensions
    {
        public static string ReadPath(this InputPacketBuffer data)
        {
            string path = "";
            byte count = data.ReadByte();
            for (int i = 0; i < count; i++)
            {
                path = Path.Combine(path, data.ReadString16());
            }

            return path;
        }
        public static void WritePath(this OutputPacketBuffer packet, string input_path)
        {
            string[] path;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = input_path.Split('\\');
            else
                path = input_path.Split('/');

            packet.WriteByte((byte)path.Length);

            foreach (var item in path)
            {
                packet.WriteString16(item);
            }
        }
    }
}
