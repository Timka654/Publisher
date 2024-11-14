using ServerPublisher.Server.Managers;
using System;
using System.Diagnostics;

namespace ServerPublisher.Server.Info
{
    internal class ProjectServiceInfo
    {
        private readonly ServiceManager serviceManager;

        private ISystemServiceProvider serviceProvider => serviceManager.Provider;

        public string ServiceName => ProjectInfo.Info.Variables.TryGetValue("service.name", out var value) ? value.Trim() : default;

        public ServerProjectInfo ProjectInfo { get; }

        public Process ServiceProcess { get; set; }

        public ProjectServiceInfo(ServerProjectInfo projectInfo, ServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;

            this.ProjectInfo = projectInfo;
        }

        internal bool StartService()
            => serviceProvider?.StartService(this) ?? false;

        internal bool StopService()
            => serviceProvider?.StopService(this) ?? false;

        internal string StatusService()
            => serviceProvider?.StatusService(this);

        internal string JournalService(DateTime? startDate, DateTime? endDate)
            => serviceProvider?.JournalService(this, startDate, endDate);
    }
}
