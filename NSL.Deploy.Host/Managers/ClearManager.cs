﻿using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Info;
using System;
using System.IO;
using System.Threading;

namespace ServerPublisher.Server.Managers
{
    internal class ClearManager
    {
        static ClearManager instance;

        Timer timer;

        private ClearManager()
        {
            instance = this;

            timer = new Timer((e) =>
            {
                ClearSummaryLogs();
                ClearProjects();
            });

            timer.Change(TimeSpan.FromMinutes(10), TimeSpan.FromHours(4));
        }

        public static void Initialize()
        {
            if (instance != null)
                return; 
            instance = new ClearManager();
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
            foreach (var item in PublisherServer.ProjectsManager.GetProjects())
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
                    item.Delete(true);
            }
        }

    }
}
