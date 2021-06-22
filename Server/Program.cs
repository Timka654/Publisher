using Publisher.Server.Configuration;
using Publisher.Server.Network;
using Publisher.Server.Tools;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Publisher.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            ServerConfigurationManager.Initialize();

            StaticInstances.ServerLogger.SetUnhandledExCatch(true);

            if ((new Commands()).Process())
                return;

            if (args.Contains("/service"))
            {
                ServiceBase.Run(new PublisherService());
            }
            else
            {
                PublisherNetworkServer.Instance.Load();

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
