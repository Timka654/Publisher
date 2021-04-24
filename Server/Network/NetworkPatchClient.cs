using Publisher.Server.Info;
using SCL;

namespace Publisher.Server.Network
{
    public class NetworkPatchClient : BaseSocketNetworkClient, IProcessFileContainer
    {
        public ProjectFileInfo CurrentFile { get; set; }
    }
}
