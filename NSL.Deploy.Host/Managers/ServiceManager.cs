using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Managers.Storages;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(3)]
    internal class ServiceManager : ServiceStorage
    {
        public static ServiceManager Instance { get; private set; }

        static ConfigurationSettingsInfo configuration => PublisherServer.Configuration;

        public ISystemServiceProvider Provider { get; }

        public ServiceManager() : base()
        {
            Instance = this;

            // linux first
            if (configuration.Publisher.Service.UseIntegrate)
                Provider = null; // new IntegratedServiceProvider(this);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Provider = new LinuxServiceProvider(this);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Provider = null; // new WindowsServiceProvider(this);
        }

        public void TryRegisterService(ServerProjectInfo project)
        {
            var service = Provider?.RegisterService(project);

            if (service == null)
                return;

            if (AddService(service))
                return;

            service.StartService();
        }

        public bool UnregisterService(ServerProjectInfo project)
        {
            var service = GetService(project);

            if (service != null)
                return false;

            if (Provider?.UnregisterService(service) != true)
                return false;

            RemoveService(service);

            return true;
        }

        public bool StartService(ServerProjectInfo project)
        {
            var service = GetService(project);

            if (service == null)
                return false;

            return service.StartService();
        }

        public bool StopService(ServerProjectInfo project)
        {
            var service = GetService(project);

            if (service == null)
                return false;

            return service.StopService();
        }

        public string StatusService(ServerProjectInfo project)
        {
            var service = GetService(project);

            if (service == null)
                return null;

            return service.StatusService();
        }

        public string JournalService(ServerProjectInfo project, DateTime? startDate, DateTime? endDate)
        {
            var service = GetService(project);

            if (service == null)
                return null;

            return service.JournalService(startDate, endDate);
        }
    }

    internal interface ISystemServiceProvider
    {
        ProjectServiceInfo RegisterService(ServerProjectInfo project);

        bool UnregisterService(ProjectServiceInfo project);

        bool StartService(ProjectServiceInfo service);

        string StatusService(ProjectServiceInfo service);

        string JournalService(ProjectServiceInfo service, DateTime? startDate, DateTime? endDate);

        bool StopService(ProjectServiceInfo service);
    }

    internal class LinuxServiceProvider : ISystemServiceProvider
    {
        private readonly ServiceManager serviceManager;

        public ProjectServiceInfo RegisterService(ServerProjectInfo project)
        {
            var service = new ProjectServiceInfo(project, serviceManager);

            if (string.IsNullOrWhiteSpace(service.ServiceName))
                return null;

            if (BashExec($"sudo systemctl enable {service.ServiceName}.service"))
                return service;

            return null;
        }

        public bool UnregisterService(ProjectServiceInfo service)
        {
            if (!service.StopService())
                return false;

            if (!BashExec($"sudo systemctl disable {service.ServiceName}.service"))
                return false;

            return true;
        }

        public bool StartService(ProjectServiceInfo service)
        {
            if (!BashExec($"sudo systemctl start {service.ServiceName}.service"))
                return false;

            return true;
        }

        public bool StopService(ProjectServiceInfo service)
        {
            if (!BashExec($"sudo systemctl stop {service.ServiceName}.service"))
                return false;

            return true;
        }

        public string StatusService(ProjectServiceInfo service)
        {
            var output = new StringBuilder();

            BashExecRead($"sudo systemctl status {service.ServiceName}.service", (line) => output.AppendLine(line));

            return output.ToString();
        }

        public string JournalService(ProjectServiceInfo service, DateTime? startDate, DateTime? endDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("journalctl -u ");
            sb.Append(service.ServiceName);

            if (startDate.HasValue)
                sb.Append($" --since \"{startDate.Value.ToString("yyyy-MM/dd/ HH:mm")}\"");
            else
                sb.Append($" --since \"today\"");

            if (endDate.HasValue)
                sb.Append($" --until \"{endDate.Value.ToString("yyyy-MM/dd/ HH:mm")}\"");

            var output = new StringBuilder();

            BashExecRead(sb.ToString(), (line) => output.AppendLine(line));

            return output.ToString();
        }

        private static bool BashExec(string command, int successExitCode = 0)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            Process cmdProc = Process.Start("/bin/bash", $"-c \"{command}\"");

            cmdProc.WaitForExit();

            return cmdProc.ExitCode == successExitCode;
        }

        private static bool BashExecRead(string command, Action<string> onReceive, int successExitCode = 0)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            var pi = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
            {
                RedirectStandardOutput = true,
            };

            Process cmdProc = Process.Start(pi);

            cmdProc.OutputDataReceived += (s, e) =>
            {
                onReceive(e.Data);
            };

            cmdProc.BeginOutputReadLine();

            cmdProc.WaitForExit();

            return cmdProc.ExitCode == successExitCode;
        }

        public LinuxServiceProvider(ServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
    }

    // linux first
    //public class WindowsServiceProvider : ISystemServiceProvider
    //{ 

    //}

    //public class IntegratedServiceProvider : ISystemServiceProvider { }
}
