using Publisher.Server.Info;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Network
{
    public class NetworkClient : IServerNetworkClient
    {
        public UserInfo UserInfo { get; set; }
        public ProjectInfo ProjectInfo => UserInfo.CurrentProject;

        public ProjectFileInfo CurrentFile { get; set; }
    }
}
