
using NSL.Logger;
using ServerPublisher.Server.Network.PublisherClient;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace ServerPublisher.Server
{
    [SupportedOSPlatform("windows")]
    class PublisherService : ServiceBase
    {
        public PublisherService()
        {
            PublisherNetworkServer.Initialize();
        }

        protected override void OnStart(string[] args)
        {
            PublisherServer.ServerLogger.AppendInfo("Service:Starting");

            PublisherNetworkServer.Run();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            PublisherServer.ServerLogger.AppendInfo("Service:Stopping");
            PublisherNetworkServer.Stop();
            base.OnStop();
        }

        protected override void OnContinue()
        {
            PublisherServer.ServerLogger.AppendInfo("Service:Continue");
            PublisherNetworkServer.Run();
            base.OnContinue();
        }

        protected override void OnPause()
        {
            PublisherServer.ServerLogger.AppendInfo("Service:Pausing");
            PublisherNetworkServer.Stop();
            base.OnPause();
        }
    }
}

