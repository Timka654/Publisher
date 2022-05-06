﻿using ServerPublisher.Server.Network.PublisherClient;
using System.ServiceProcess;

namespace ServerPublisher.Server
{
    class PublisherService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Starting");
            PublisherNetworkServer.Instance.Load();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Stopping");
            PublisherNetworkServer.Listener.Stop();
            base.OnStop();
        }

        protected override void OnContinue()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Continue");
            PublisherNetworkServer.Listener.Run();
            base.OnContinue();
        }

        protected override void OnPause()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Pausing");
            PublisherNetworkServer.Listener.Stop();
            base.OnPause();
        }
    }
}