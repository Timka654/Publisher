using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Shared.Enums;
using System;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(2)]
    public class ExplorerManager
    {
        public static ExplorerManager Instance { get; private set; }

        public ExplorerManager()
        {
            Instance = this;
        }

        internal ExplorerActionResultEnum DownloadFile(string projectId, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
