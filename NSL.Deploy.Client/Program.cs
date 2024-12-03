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

        public static string ConfigurationPath { get; private set; }

        static void Main(string[] args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            ConfigurationPath = Path.Combine(appPath, "config.json");

            if (File.Exists(ConfigurationPath))
                Configuration = JsonConvert.DeserializeObject<ConfigurationInfoModel>(File.ReadAllText(ConfigurationPath));

            Commands.Process();
        }
    }
}
