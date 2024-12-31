using NSL.Logger;
using NSL.Utils.CommandLine.CLHandles;
using ServerPublisher.Server;

namespace NSL.Deploy.Host.Utils.Commands
{
    internal class AppCommands : CLHandler
    {
        public AppCommands()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("default", true));
        }

        public static FileLogger Logger => PublisherServer.ServerLogger;
    }
}
