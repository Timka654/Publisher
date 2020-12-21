using SCL;

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
