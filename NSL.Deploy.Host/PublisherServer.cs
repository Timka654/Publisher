using Newtonsoft.Json;
using NSL.Logger;
using NSL.Utils;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPublisher.Server
{
    internal class PublisherServer
    {
        //public static IConfiguration Configuration { get; private set; }

        public static ConfigurationSettingsInfo Configuration { get; private set; }


#if RELEASE
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "server"), handleUnhandledThrow: true);
#else
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine("logs", "server").GetNormalizedPath(), handleUnhandledThrow: true);
#endif


        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static ProjectProxyManager ProjectProxyManager => ProjectProxyManager.Instance;

        public static bool ServiceInvokable { get; set; } = false;

        public static void InitializeApp(string appPath)
        {
            ServerLogger.SetUnhandledExCatch(true);

            ServerLogger.AppendInfo($"Initialize application");

            initializeConfiguration();

            IOUtils.CreateDirectoryIfNoExists(Path.Combine(appPath, Configuration.Publisher.ProjectConfiguration.Server.GlobalScriptsFolderPath));
        }

        static void initializeConfiguration()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "ServerSettings.json");

            if (!File.Exists(path))
            {
                ServerLogger.AppendError($"Cannot found \"{path}\" configuration file - load default!!");

                Configuration = new ConfigurationSettingsInfo();

                return;
            }

            Configuration = JsonConvert.DeserializeObject<ConfigurationSettingsInfo>(File.ReadAllText(path));
        }

        public static async Task RunServer()
        {
#if DEBUG
            PublisherNetworkServer.Initialize();
            PublisherNetworkServer.Run();
            await Task.Delay(Timeout.Infinite);
#endif

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ServiceBase.Run(new PublisherService());
            else
            {
                PublisherNetworkServer.Initialize();
                PublisherNetworkServer.Run();

                await Task.Delay(Timeout.Infinite);
            }
        }
    }
}