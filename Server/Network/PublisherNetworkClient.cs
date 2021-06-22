using Publisher.Server.Info;
using SocketServer.Utils;
using System.Collections.Generic;

namespace Publisher.Server.Network
{
    public class PublisherNetworkClient : IServerNetworkClient, IProcessFileContainer
    {
        public UserInfo UserInfo { get; set; }
        public ServerProjectInfo ProjectInfo => UserInfo.CurrentProject;

        public ProjectFileInfo CurrentFile { get; set; }

        public bool IsPatchClient { get; set; } = false;

        public ServerProjectInfo PatchDownloadProject { get; set; }

        public Dictionary<string, ServerProjectInfo> PatchProjectMap { get; set; } = null;
    }
}
