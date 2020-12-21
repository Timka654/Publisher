using Publisher.Server.Managers.Storages;
using Publisher.Server.Network;
using ServerOptions.Extensions.Manager;
using System.IO;

namespace Publisher.Server.Managers
{
    [ManagerLoad(0)]
    public class SessionManager : SessionsStorage
    {
        public static SessionManager Instance { get; private set; }

        public DirectoryInfo UsersDirectory { get; private set; }

        private FileSystemWatcher directoryWatcher;


        public SessionManager()
        {
            Instance = this;
            StaticInstances.ServerLogger.AppendInfo("Load users");
            //LoadUsers();
            //LoadWatcher();
            StaticInstances.ServerLogger.AppendInfo("Load users finished");
        }

        public void DisconnectClient(NetworkClient client)
        {
            if (client?.UserInfo == null)
                return;

            RemoveUser(client.UserInfo);

            if (client.ProjectInfo != null)
                client.ProjectInfo.StopProcess(client,false);


            client.CurrentFile?.EndFile();


            client.UserInfo = null;
            client.Network?.Disconnect();
        }

        //private void LoadWatcher()
        //{
        //    directoryWatcher = new FileSystemWatcher(UsersDirectory.FullName, "*.json");
        //    directoryWatcher.Deleted += DirectoryWatcher_Deleted;
        //    directoryWatcher.Changed += DirectoryWatcher_Changed;
        //    directoryWatcher.EnableRaisingEvents = true;
        //}

        //private void DirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    if (e.ChangeType != WatcherChangeTypes.Changed)
        //        return;

        //    UserInfo userInfo = new UserInfo(e.FullPath);

        //    if (AddUser(userInfo))
        //    {
        //        StaticInstances.ServerLogger.AppendInfo($"User {userInfo.Name}({userInfo.Id}) - appended");
        //    }
        //    else
        //    {
        //        UserInfo exist = GetUser(userInfo);
        //        exist.Reload(userInfo);
        //        StaticInstances.ServerLogger.AppendInfo($"User {userInfo.Name}({userInfo.Id}) - reloaded");
        //    }
        //}

        //private void DirectoryWatcher_Deleted(object sender, FileSystemEventArgs e)
        //{
        //    var proj = base.userStorage.Values.FirstOrDefault(x => x.FileName == e.FullPath);

        //    StaticInstances.ServerLogger.AppendInfo($"User file removed {e.FullPath}");

        //    if (proj == null)
        //    {
        //        StaticInstances.ServerLogger.AppendInfo($"User not found!");
        //        return;
        //    }

        //    RemoveUser(proj);

        //    StaticInstances.ServerLogger.AppendInfo($"User {proj.Name}({proj.Id}) - removed");
        //}

        //private void LoadUsers()
        //{
        //    UsersDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));
        //    if (!UsersDirectory.Exists)
        //    {
        //        UsersDirectory.Create();
        //        return;
        //    }

        //    foreach (var item in LoadUsers(UsersDirectory))
        //    {
        //        AddUser(item);
        //        StaticInstances.ServerLogger.AppendInfo($"User {item.Id} - {item.Name} loaded");
        //    }
        //}

        //public static List<UserInfo> LoadUsers(DirectoryInfo usrDir)
        //{
        //    List<UserInfo> result = new List<UserInfo>();
        //    foreach (var item in usrDir.GetFiles("*.priuk"))
        //    {
        //        result.Add(new UserInfo(item.FullName));
        //    }

        //    return result;
        //}
    }
}
