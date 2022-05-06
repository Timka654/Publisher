using NSL.Logger;
using ServerPublisher.Server.Configuration;
using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Network.PublisherClient;
#if RELEASE
using System;
using System.IO;
#endif

namespace ServerPublisher.Server
{
    internal class StaticInstances
    {
        public static ServerConfigurationManager ServerConfiguration => ServerConfigurationManager.Instance;

#if RELEASE
        public static FileLogger ServerLogger { get; } = FileLogger.Initialize(Path.Combine(Application.Directory, "logs/server"));
#else
        public static FileLogger ServerLogger { get; } = FileLogger.Initialize("logs/server");
#endif


        public static PublisherNetworkServer Server => PublisherNetworkServer.Instance;

        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static PatchManager PatchManager => PatchManager.Instance;

        public static SessionManager SessionManager => SessionManager.Instance;

        public static bool CommandExecutor { get; set; } = false;
    }
}
