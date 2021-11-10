using Publisher.Server.Info;
using ServerOptions.Extensions.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher.Server.Managers
{
    [ManagerLoad(0)]
    internal class ClearManager
    {
        Timer timer;
        public ClearManager()
        {
            timer = new Timer((e) =>
            {
                ClearSummaryLogs();
                ClearProjects();
            });

            timer.Change(TimeSpan.FromMinutes(10), TimeSpan.FromHours(4));
        }

        private void ClearSummaryLogs()
        {
            var logsDir = Path.Combine("logs", "server");

            var dir = new DirectoryInfo(logsDir);

            if (!dir.Exists)
                return;

            DateTime minValue = DateTime.UtcNow.AddDays(-10);

            foreach (var item in dir.GetFiles("*.log"))
            {
                if (item.CreationTimeUtc < minValue)
                    item.Delete();
            }
        }

        private void ClearProjects()
        {
            foreach (var item in StaticInstances.ProjectsManager.GetProjects())
            {
                ClearProjectLogs(item);
                ClearProjectBackups(item);
            }
        }

        private void ClearProjectLogs(ServerProjectInfo proj)
        {
            var dir = new DirectoryInfo(proj.LogsDirPath);

            if (!dir.Exists)
                return;

            DateTime minValue = DateTime.UtcNow.AddDays(-10);

            foreach (var item in dir.GetFiles("*.log"))
            {
                if (item.CreationTimeUtc < minValue)
                    item.Delete();
            }
        }

        private void ClearProjectBackups(ServerProjectInfo proj)
        {
            var dir = new DirectoryInfo(proj.ProjectBackupPath);

            if (!dir.Exists)
                return;

            DateTime minValue = DateTime.UtcNow.AddDays(-10);

            foreach (var item in dir.GetDirectories())
            {
                if (item.CreationTimeUtc < minValue)
                    item.Delete();
            }
        }

    }
}
