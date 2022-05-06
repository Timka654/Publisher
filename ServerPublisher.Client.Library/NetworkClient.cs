using NSL.SocketClient;

namespace ServerPublisher.Client.Library
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
