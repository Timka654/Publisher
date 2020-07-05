using ConfigurationEngine;
using ConfigurationEngine.Info;
using ConfigurationEngine.Providers.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Configuration
{
    public class ServerConfigurationManager : IConfigurationManager<ServerConfigurationManager>
    {
        private static readonly List<ConfigurationInfo> DefaultValues = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo("server/io.ip","0.0.0.0",""),
            new ConfigurationInfo("server/io.port","578",""),
            new ConfigurationInfo("server/io.backlog","5",""),
            new ConfigurationInfo("server/io.ipv","4",""),
            new ConfigurationInfo("server/io.protocol","tcp",""),
            new ConfigurationInfo("server/io.buffer.size","20480",""),
            new ConfigurationInfo("server/io.input.key","!{b1HX11R**",""),
            new ConfigurationInfo("server/io.output.key","!{b1HX11R**",""),
            new ConfigurationInfo("paths/projects_library","proj_lib.json",""),
            new ConfigurationInfo("upload/large.min.size","400000000",""),
            new ConfigurationInfo("upload/max.file.size","1000000000",""),
            new ConfigurationInfo("upload/max.file.count","999999999",""),

        };

        public static ServerConfigurationManager Instance;

        public static void Initialize()
        {
            StaticInstances.ServerLogger.AppendInfo("Loading server configuration");
               Instance = new ServerConfigurationManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json"));
            Instance.OnLog += StaticInstances.ServerLogger.Append;
            Instance.SetDefaults(DefaultValues, true);
        }

        public ServerConfigurationManager(string fileName, char nodeSeparator = '/') : base(fileName, nodeSeparator)
        {
            Provider = new LoadingProvider();
        }
    }
}
