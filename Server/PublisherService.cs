using Publisher.Server.Network;
using System.ServiceProcess;

namespace Publisher.Server
{
    class PublisherService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Starting");
            NetworkServer.Start();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Stopping");
            NetworkServer.Stop();
            base.OnStop();
        }

        protected override void OnContinue()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Continue");
            NetworkServer.Start();
            base.OnContinue();
        }

        protected override void OnPause()
        {
            StaticInstances.ServerLogger.AppendInfo("Service:Pausing");
            NetworkServer.Stop();
            base.OnPause();
        }
    }
}
