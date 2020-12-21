﻿using SCLogger;
using Publisher.Server.Configuration;
using Publisher.Server.Managers;
using Publisher.Server.Network;

namespace Publisher.Server
{
    public class StaticInstances
    {
        public static ServerConfigurationManager ServerConfiguration => ServerConfigurationManager.Instance;

        public static FileLogger ServerLogger { get; } = FileLogger.Initialize("logs/server");

        public static NetworkServer Server => NetworkServer.Instance;

        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        public static SessionManager SessionManager => SessionManager.Instance;

        public static bool CommandExecutor { get; set; } = false;
    }
}
