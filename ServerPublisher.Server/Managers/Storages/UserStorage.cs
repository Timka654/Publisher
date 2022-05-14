﻿using ServerPublisher.Server.Info;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Managers.Storages
{
    public class UserStorage
    {
        protected static readonly string DataDirPath = Path.Combine(Application.Directory, "Data");
        protected static readonly string UsersDirPath = Path.Combine(DataDirPath, "Users");

        protected List<UserInfo> userList = new List<UserInfo>();

        public UserInfo GetUser(string userId)
            => userList.Find(x => x.Id == userId);

        protected UserStorage()
        {
            CreateWather();
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (!File.Exists(UsersDirPath))
                return;

            foreach (var item in Directory.GetFiles(UsersDirPath, "*.priuk"))
            {
                AddOrUpdateUser(new UserInfo(item));
            }
        }

        private FileSystemWatcher UsersWatch;

        private void CreateWather()
        {
            string path = UsersDirPath;
            
            if (!Directory.Exists(path))
                path = DataDirPath;

            if (!Directory.Exists(path))
                path = Application.Directory;

            UsersWatch = new FileSystemWatcher(path, "*.priuk");

            UsersWatch.Changed += UsersWatch_Changed;
            UsersWatch.Deleted += UsersWatch_Deleted;

            UsersWatch.EnableRaisingEvents = true;
        }

        private void UsersWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            userList.RemoveAll(x => x.FileName == e.FullPath);
        }

        private async void UsersWatch_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(1500);

            try
            {
                AddOrUpdateUser(new UserInfo(e.FullPath));
            }
            catch { }
        }

        private void AddOrUpdateUser(UserInfo user)
        {
            var exist = userList.Find(x => x.Id == user.Id);

            if (exist == null)
            {
                userList.Add(user);
            }
            else
            {
                exist.Reload(user);
            }
        }
    }
}
