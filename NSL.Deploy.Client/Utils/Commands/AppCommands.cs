using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Utils.CommandLine.CLHandles;

namespace NSL.Deploy.Client.Utils.Commands
{
    internal class AppCommands : CLHandler
    {
        public static ILogger Logger { get; } = ConsoleLogger.Create();

        public AppCommands()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("default", true));
        }
    }
}
