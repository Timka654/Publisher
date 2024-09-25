using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSL.Logger;
using NSL.SocketCore.Utils.Logger;
using ServerPublisher.Server.Managers;
using ServerPublisher.Server.Network.PublisherClient;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace ServerPublisher.Server
{
    internal class PublisherServer
    {
        public static IConfiguration Configuration { get; private set; }

        public static IBasicLogger AppLogger { get; private set; }


#if RELEASE
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine(Application.Directory, "logs", "server"), handleUnhandledThrow: true);
#else
        public static FileLogger ServerLogger { get; } = new FileLogger(Path.Combine("logs", "server"), handleUnhandledThrow: true);
#endif


        public static ProjectsManager ProjectsManager => ProjectsManager.Instance;

        internal static ProjectProxyManager ProjectProxyManager => ProjectProxyManager.Instance;

        public static ServiceManager ServiceManager => ServiceManager.Instance;

        public static ExplorerManager ExplorerManager => ExplorerManager.Instance;

        public static UserManager UserManager => UserManager.Instance;

        public static bool CommandExecutor { get; set; } = false;

        static Dictionary<string, string> defaultConfiguration = new Dictionary<string, string>()
        {
            {"publisher:network:binding_port","6583"},
            {"publisher:network:backlog","100"},
            {"server.publisher.io.buffer.size","409600"},
            {"server.publisher.io.input.key","!{b1HX11R**"},
            {"server.publisher.io.output.key","!{b1HX11R**"},
            {"paths.projects_library", Path.Combine("Data", "Projects.json")},
            {"patch.io.buffer.size","409600"},
            {"service.use_integrate","false"},
        };

        public static void InitializeApp()
        {
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(defaultConfiguration)
                .AddJsonFile("ServerSettings.json")
                    .Build();

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

            var logger = loggerFactory.CreateLogger("Application");

            AppLogger = new NSL.Logger.AspNet.ILoggerWrapper(logger);
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

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}