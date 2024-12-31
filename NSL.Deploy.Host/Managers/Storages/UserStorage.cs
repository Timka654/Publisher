using NSL.Logger;
using NSL.Utils;
using ServerPublisher.Server.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Managers.Storages
{
    public class UserStorage
    {
        protected List<UserInfo> userList = new List<UserInfo>();

        public UserInfo? GetUser(string userId)
            => userList.Find(x => x.Id == userId);

        public IReadOnlyList<UserInfo> Users => userList;

        public string DirPath { get; private set; }
        private string watchPattern;

        public event Action<UserInfo> OnCreated = (f) => { };

        public UserStorage(string dirPath, string watchPattern = "*.priuk")
        {
            this.DirPath = dirPath;
            this.watchPattern = watchPattern;

            IOUtils.CreateDirectoryIfNoExists(dirPath);

            CreateWatcher();
            LoadUsers();
        }

        private void LoadUsers()
        {
            foreach (var item in Directory.GetFiles(DirPath, watchPattern, SearchOption.AllDirectories))
            {
                AddOrUpdateUser(new UserInfo(item));
            }
        }

        private FSWatcher[] UsersWatch;

        private void CreateWatcher()
        {
            if (PublisherServer.CommandExecutor)
                return;

            UsersWatch = [ new FSWatcher(DirPath, watchPattern, onCreated: UsersWatch_Changed, onChanged: UsersWatch_Changed, onDeleted: UsersWatch_Deleted)];
        }

        private void UsersWatch_Deleted(FileSystemEventArgs e)
        {
            userList.RemoveAll(userList.Where(x =>
            {
                if (x.FileName != e.FullPath) return false;
                x.Dispose(); return true;
            }).ToArray().Contains);
        }

        private void UsersWatch_Changed(FileSystemEventArgs e)
        {
            AddOrUpdateUser(new UserInfo(e.FullPath));
        }

        private void AddOrUpdateUser(UserInfo user)
        {
            var exist = userList.Find(x => x.Id == user.Id);

            if (exist == null)
            {
                userList.Add(user);
                OnCreated(user);
            }
            else
            {
                exist.Reload(user);
            }
        }

        internal bool AddUser(UserInfo user)
        {
            if (userList.Any(x => x.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            user.ProducePublicKey(Path.Combine(DirPath, "publ"));
            user.ProducePrivateKey(DirPath);

            return true;
        }
    }
}
