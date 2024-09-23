using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Managers.Storages;
using ServerPublisher.Server.Network.PublisherClient;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(0)]
    public class SessionManager : SessionsStorage
    {
        public static SessionManager Instance { get; private set; }

        //public DirectoryInfo UsersDirectory { get; private set; }

        //private FileSystemWatcher directoryWatcher;

        public SessionManager()
        {
            Instance = this;
            PublisherServer.ServerLogger.AppendInfo("Load users");
            PublisherServer.ServerLogger.AppendInfo("Load users finished");
        }

        public void DisconnectClient(PublisherNetworkClient client)
        {
            if (client?.UserInfo != null)
            {
                RemoveUser(client.UserInfo);

                if (client.ProjectInfo != null)
                {
                    client.CurrentFile?.EndFile();
                    client.ProjectInfo.StopProcess(client, false);
                }
            }
            if (client.IsPatchClient)
            {
                foreach (var item in client.PatchProjectMap)
                {
                    item.Value.SignOutPatchClient(client);
                }
                client.PatchProjectMap.Clear();
            }

            client.UserInfo = null;
            client.Network?.Disconnect();
            client.Dispose();
        }
    }
}
