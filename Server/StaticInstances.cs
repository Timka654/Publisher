using SCLogger;
using Publisher.Server.Configuration;
using Publisher.Server.Managers;
using Publisher.Server.Network;
using Publisher.Server._.Managers;

namespace Publisher.Server
{
    internal class StaticInstances
    {
        public static ServerConfigurationManager ServerConfiguration => ServerConfigurationManager.Instance;

        public static FileLogger ServerLogger { get; } = FileLogger.Initialize("logs/server");

        public static PublisherNetworkServer Server => PublisherNetworkServer.Instance;

        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static PatchManager PatchManager => PatchManager.Instance;

        public static SessionManager SessionManager => SessionManager.Instance;

        public static bool CommandExecutor { get; set; } = false;
    }
}
