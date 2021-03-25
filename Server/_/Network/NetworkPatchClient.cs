using Publisher.Server._.Info;
using Publisher.Server.Info;
using SCL;
using SocketCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server._.Network
{
    public class NetworkPatchClient : BaseSocketNetworkClient, IProcessFileContainer
    {
        public ProjectFileInfo CurrentFile { get; set; }
    }
}
