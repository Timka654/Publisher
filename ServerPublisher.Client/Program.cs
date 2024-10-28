using Newtonsoft.Json;
using ServerPublisher.Client.Utils;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;

namespace ServerPublisher.Client
{
    class Program
    {
        public static ConfigurationInfoModel Configuration { get; private set; } = new ConfigurationInfoModel();

        static void Main(string[] args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            var configurationPath = Path.Combine(appPath, "config.json");

            if (File.Exists(configurationPath))
                Configuration = JsonConvert.DeserializeObject<ConfigurationInfoModel>(File.ReadAllText(configurationPath));
            else
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(Configuration));

            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.ExpandEnvironmentVariables(Configuration.TemplatesPath))))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.ExpandEnvironmentVariables(Configuration.TemplatesPath)));

            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.ExpandEnvironmentVariables(Configuration.KeysPath))))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.ExpandEnvironmentVariables(Configuration.KeysPath)));

            Commands.Process();
        }
    }
}
