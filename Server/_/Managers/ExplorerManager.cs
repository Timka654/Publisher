using Publisher.Basic;
using ServerOptions.Extensions.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
