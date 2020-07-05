using Publisher.Server.Configuration;
using Publisher.Server.Network;
using Publisher.Server.Tools;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Publisher.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServerConfigurationManager.Initialize();


            if (Commands.Process())
                return;

            if (args.Contains("/service"))
            {
                ServiceBase.Run(new PublisherService());
            }
            else
            {
                NetworkServer.Start();

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
