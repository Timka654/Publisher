using Publisher.Basic;
using ServerOptions.Extensions.Manager;
using System;

namespace Publisher.Server.Managers
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
