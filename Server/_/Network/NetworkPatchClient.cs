using Publisher.Server._.Info;
using Publisher.Server.Info;
using SCL;

namespace Publisher.Server._.Network
{
    public class NetworkPatchClient : BaseSocketNetworkClient, IProcessFileContainer
    {
        public ProjectFileInfo CurrentFile { get; set; }
    }
}
