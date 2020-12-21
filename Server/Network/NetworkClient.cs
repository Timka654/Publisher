using Publisher.Server.Info;
using SocketServer.Utils;

namespace Publisher.Server.Network
{
    public class NetworkClient : IServerNetworkClient
    {
        public UserInfo UserInfo { get; set; }
        public ProjectInfo ProjectInfo => UserInfo.CurrentProject;

        public ProjectFileInfo CurrentFile { get; set; }

    }
}
