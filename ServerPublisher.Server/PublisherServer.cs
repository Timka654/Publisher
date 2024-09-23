using Microsoft.Extensions.Configuration;
using NSL.Logger;
using NSL.Logger.Interface;
using ServerPublisher.Server.Managers;
using System.IO;

namespace ServerPublisher.Server
{
    internal class PublisherServer
    {
        public static IConfiguration Configuration { get; private set; }

        public static ILogger AppLogger { get; private set; }


#if RELEASE
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine(Application.Directory, "logs", "server"), handleUnhandledThrow: true);
#else
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine("logs", "server"), handleUnhandledThrow: true);
#endif


        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static ProjectProxyManager ProjectProxyManager => ProjectProxyManager.Instance;

        public static SessionManager SessionManager => SessionManager.Instance;

        public static ServiceManager ServiceManager => ServiceManager.Instance;

        public static ExplorerManager ExplorerManager => ExplorerManager.Instance;

        public static UserManager UserManager => UserManager.Instance;

        public static bool CommandExecutor { get; set; } = false;
    }
}
