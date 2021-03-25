using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Basic
{
    public class ProjectPatchInfo
    {
        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string SignName { get; set; }

        public string InputCipherKey { get; set; }

        public string OutputCipherKey { get; set; }
    }
}
