using ServerPublisher.Server.Configuration;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Server.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerPublisher.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServerConfigurationManager.Initialize();

            StaticInstances.ServerLogger.SetUnhandledExCatch(true);
            if ((new Commands()).Process())
                return;

            if (args.Contains("/service"))
            {
#if RELEASE
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    ServiceBase.Run(new PublisherService());
                else
                {
#endif
                    PublisherNetworkServer.Instance.Load();

                    Thread.Sleep(Timeout.Infinite);
#if RELEASE
                }
#endif
                return;
            }

            Console.WriteLine($"Unknown args {string.Join(" ", args)}");
        }
    }

    public class Application
    {
        public static string Directory = Debugger.IsAttached ? System.IO.Directory.GetCurrentDirectory() :
            (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.CurrentDirectory : AppDomain.CurrentDomain.BaseDirectory);
    }
}
