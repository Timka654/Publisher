using ConfigurationEngine;
using ConfigurationEngine.Info;
using ConfigurationEngine.Providers.Json;
using System.Collections.Generic;
using System.IO;

namespace Publisher.Server.Configuration
{
    public class ServerConfigurationManager : IConfigurationManager<ServerConfigurationManager>
    {
        private static readonly List<ConfigurationInfo> DefaultValues = new List<ConfigurationInfo>()
        {
            new ConfigurationInfo("server.io.ip","0.0.0.0",""),
            new ConfigurationInfo("server.io.port","6583",""),
            new ConfigurationInfo("server.io.backlog","10",""),
            new ConfigurationInfo("server.io.ipv","4",""),
            new ConfigurationInfo("server.io.protocol","tcp",""),
            new ConfigurationInfo("server.io.buffer.size","61440",""),
            new ConfigurationInfo("server.io.input.key","!{b1HX11R**",""),
            new ConfigurationInfo("server.io.output.key","!{b1HX11R**",""),
            new ConfigurationInfo("paths.projects_library","proj_lib.json",""),
            new ConfigurationInfo("upload.large.min.size","400000000",""),
            new ConfigurationInfo("upload.max.file.size","1000000000",""),
            new ConfigurationInfo("upload.max.file.count","999999999",""),
            new ConfigurationInfo("patch_server.enabled","false",""),
            new ConfigurationInfo("patch.io.buffer.size","61440",""),
        };

        public static ServerConfigurationManager Instance;

        public static void Initialize()
        {
            StaticInstances.ServerLogger.AppendInfo("Loading server configuration");
            string dir = "";
#if RELEASE
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dir = Environment.CurrentDirectory;
            }
            else
            {
            dir = AppDomain.CurrentDomain.BaseDirectory;
            }
#else
            dir = Directory.GetCurrentDirectory();
#endif

            if (File.Exists(Path.Combine(dir, "ServerSettings.json")) == false)
                File.WriteAllText(Path.Combine(dir, "ServerSettings.json"), "{}");

            Instance = new ServerConfigurationManager(Path.Combine(dir, "ServerSettings.json"));
            Instance.OnLog += StaticInstances.ServerLogger.Append;
            Instance.SetDefaults(DefaultValues, true);
        }

        public ServerConfigurationManager(string fileName) : base(fileName)
        {
            Provider = new LoadingProvider();
        }
    }
}
