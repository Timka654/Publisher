using SCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
