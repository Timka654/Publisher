using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSL.Logger;
using NSL.SocketCore.Utils.Logger;
using ServerPublisher.Server.Dev.Test.Utils;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace ServerPublisher.Server
{
    internal class PublisherServer
    {
        //public static IConfiguration Configuration { get; private set; }

        public static ConfigurationSettingsInfo Configuration { get; private set; }


#if RELEASE
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine(Application.Directory, "logs", "server"), handleUnhandledThrow: true);
#else
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine("logs", "server").GetNormalizedPath(), handleUnhandledThrow: true);
#endif


        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static ProjectProxyManager ProjectProxyManager => ProjectProxyManager.Instance;

        public static ServiceManager ServiceManager => ServiceManager.Instance;

        public static ExplorerManager ExplorerManager => ExplorerManager.Instance;

        public static bool CommandExecutor { get; set; } = false;

        public static void InitializeApp()
        {
            ServerLogger.AppendInfo($"Initialize application");

            initializeConfiguration();

            //var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

            //var logger = loggerFactory.CreateLogger("Application");

            //AppLogger = new NSL.Logger.AspNet.ILoggerWrapper(logger);

            DirectoryUtils.CreateNoExistsDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.Publisher.ProjectConfiguration.Server.GlobalScriptsFolderPath));
        }

        static void initializeConfiguration()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json");

            if (!File.Exists(path))
            {
                ServerLogger.AppendError($"Cannot found \"{path}\" configuration file - load default!!");

                Configuration = new ConfigurationSettingsInfo();

                return;
            }

            Configuration = JsonConvert.DeserializeObject<ConfigurationSettingsInfo>(File.ReadAllText(path));
        }

        public static void RunServer()
        {
#if DEBUG
            PublisherNetworkServer.Initialize();
            PublisherNetworkServer.Run();
            Thread.Sleep(Timeout.Infinite);
#endif

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ServiceBase.Run(new PublisherService());
            else
            {
                PublisherNetworkServer.Initialize();
                PublisherNetworkServer.Run();

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}