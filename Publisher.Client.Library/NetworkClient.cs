using SCL;
using SocketCore.Utils;

namespace Publisher.Client
{
    public class NetworkClient : BaseSocketNetworkClient
    {
        public static NetworkClient Instance { get; private set; }

        public NetworkClient()
        {
            Instance = this;
        }
    }
}
