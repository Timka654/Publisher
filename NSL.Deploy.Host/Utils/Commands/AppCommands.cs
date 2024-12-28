using NSL.Utils;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Network.PublisherClient;
using NSL.SocketServer;
using NSL.Logger;
using Microsoft.Extensions.Configuration;
using NSL.Utils.CommandLine.CLHandles;
using Microsoft.Extensions.Logging;
using ServerPublisher.Server;

namespace NSL.Deploy.Host.Utils.Commands
{
    internal class AppCommands : CLHandler
    {
        public AppCommands()
        {

        }

        public static FileLogger Logger => PublisherServer.ServerLogger;
    }
}
