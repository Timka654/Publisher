using Newtonsoft.Json;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Utils.CommandLine.CLHandles;
using ServerPublisher.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace NSL.Deploy.Client.Utils.Commands
{
    internal class AppCommands : CLHandler
    {
        public static ILogger Logger { get; } = ConsoleLogger.Create();

        public AppCommands()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("default", true));
        }

        public static void InitData(string path)
        {
            var configurationPath = Path.Combine(path, "config.json");

            if (!File.Exists(configurationPath))
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(Program.Configuration));

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath)));

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath)));
        }
    }
}
