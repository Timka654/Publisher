using NSL.Logger;
using ServerPublisher.Server.Configuration;
using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Network.PublisherClient;
using System.IO;

namespace ServerPublisher.Server
{
    internal class StaticInstances
    {
        public static ServerConfigurationManager ServerConfiguration => ServerConfigurationManager.Instance;

#if RELEASE
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine(Application.Directory, "logs", "server"), handleUnhandledThrow: true);
#else
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine("logs", "server"), handleUnhandledThrow: true);
#endif


        public static PublisherNetworkServer Server => PublisherNetworkServer.Instance;

        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static PatchManager PatchManager => PatchManager.Instance;

        public static SessionManager SessionManager => SessionManager.Instance;

        public static ServiceManager ServiceManager => ServiceManager.Instance;

        public static ExplorerManager ExplorerManager => ExplorerManager.Instance;
        public static UserManager UserManager => UserManager.Instance;

        public static bool CommandExecutor { get; set; } = false;
    }
}
