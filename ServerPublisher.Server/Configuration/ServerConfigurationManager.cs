using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Info;
using NSL.ConfigurationEngine.Providers.Json;
using System.Collections.Generic;
using System.IO;
#if RELEASE
#endif

namespace ServerPublisher.Server.Configuration
{
    public class ServerConfigurationManager : IConfigurationManager<ServerConfigurationManager>
    {
        private static readonly List<ConfigurationInfo> DefaultValues = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo("server.publisher.io.ip","0.0.0.0",""),
            new ConfigurationInfo("server.publisher.io.port","6583",""),
            new ConfigurationInfo("server.publisher.io.backlog","10",""),
            new ConfigurationInfo("server.publisher.io.ipv","4",""),
            new ConfigurationInfo("server.publisher.io.protocol","tcp",""),
            new ConfigurationInfo("server.publisher.io.buffer.size","409600",""),
            new ConfigurationInfo("server.publisher.io.input.key","!{b1HX11R**",""),
            new ConfigurationInfo("server.publisher.io.output.key","!{b1HX11R**",""),
            new ConfigurationInfo("paths.projects_library", Path.Combine("Data", "Projects.json"),""),
            new ConfigurationInfo("patch.io.buffer.size","409600",""),
            new ConfigurationInfo("service.use_integrate","false",""),
        };

        private static ServerConfigurationManager instance;

        public static ServerConfigurationManager Instance => instance ??= Initialize();

        public static ServerConfigurationManager Initialize()
        {
            var logger = StaticInstances.ServerLogger;
            
            logger.AppendInfo("Loading server configuration");
            
            string dir = Application.Directory;

            if (File.Exists(Path.Combine(dir, "ServerSettings.json")) == false)
                File.WriteAllText(Path.Combine(dir, "ServerSettings.json"), "{}");

            var result = new ServerConfigurationManager(Path.Combine(dir, "ServerSettings.json"));

            result.OnLog += logger.Append;
            result.SetDefaults(DefaultValues, true);

            return result;
        }

        public ServerConfigurationManager(string fileName) : base(fileName)
        {
            Provider = new LoadingProvider();
        }
    }
}
