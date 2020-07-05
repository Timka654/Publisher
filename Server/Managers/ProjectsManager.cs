using Cipher.RSA;
using Newtonsoft.Json;
using Publisher.Server.Configuration;
using Publisher.Server.Info;
using Publisher.Server.Managers.Storages;
using Publisher.Server.Network;
using ServerOptions.Extensions.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Managers
{
    [ManagerLoad(1)]
    public class ProjectsManager : ProjectsStorage
    {
        public static ProjectsManager Instance { get; private set; }

        public static string ProjectsFilePath => StaticInstances.ServerConfiguration.GetValue("paths/projects_library");

        private FileSystemWatcher directoryWatcher;

        public ProjectsManager()
        {
            Instance = this;
            StaticInstances.ServerLogger.AppendInfo("Load projects");
            LoadProjects();
            LoadWatcher();
            StaticInstances.ServerLogger.AppendInfo("Load projects finished");
        }


        internal void SignIn(NetworkClient client, string project_id, string user_id, byte[] key)
        {
            var session = StaticInstances.SessionManager.GetUser(user_id);

            if (session != null && session.CurrentNetwork != null && session.CurrentNetwork.AliveState)
            {
                Network.Packets.Project.SignIn.Send(client, Basic.SignStateEnum.AlreadyConnected);
                return;
            }

            var proj = GetProject(project_id);

            if (proj == null)
            {
                Network.Packets.Project.SignIn.Send(client, Basic.SignStateEnum.ProjectNotFound);
                return;
            }

            var user = proj.GetUser(user_id);

            if (user == null)
            {
                Network.Packets.Project.SignIn.Send(client, Basic.SignStateEnum.UserNotFound);
                return;
            }


            if (user.Cipher == null)
            {
                user.Cipher = new RSACipher();

                user.Cipher.LoadXml(user.PSAPrivateKey);
            }

            byte[] data = user.Cipher.Decode(key, 0, key.Length);

            if (Encoding.ASCII.GetString(data) == user_id)
            {
                client.UserInfo = user;
                user.CurrentNetwork = client;

                StaticInstances.SessionManager.AddUser(client.UserInfo);

                client.RunAliveChecker();
                Network.Packets.Project.SignIn.Send(client, Basic.SignStateEnum.Ok);

                proj.StartProcess(user);
                return;
            }


            Network.Packets.Project.SignIn.Send(client, Basic.SignStateEnum.UserNotFound);
        }


        private void LoadWatcher()
        {
            if (StaticInstances.CommandExecutor)
                return;

            var fi = new FileInfo(ProjectsFilePath);

            directoryWatcher = new FileSystemWatcher(fi.Directory.FullName, fi.Name);
            directoryWatcher.Deleted += DirectoryWatcher_Deleted;
            directoryWatcher.Changed += DirectoryWatcher_Changed;
            directoryWatcher.EnableRaisingEvents = true;
        }

        private void DirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;
            string json = null;

            try { json = File.ReadAllText(e.FullPath); }
            catch { return; }

            var projPathes = JsonConvert.DeserializeObject<string[]>(json);


            foreach (var item in storage.Where(x => !projPathes.Contains(x.Value.ProjectDirPath)))
            {
                RemoveProject(item.Value);
            }

            foreach (var item in projPathes)
            {
                var exist = storage.Values.FirstOrDefault(x => x.ProjectDirPath == item);

                if (exist == null)
                {
                    exist = new ProjectInfo(item);
                    AddProject(exist);
                }
            }
        }

        private void DirectoryWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            foreach (var item in storage.Values)
            {
                RemoveProject(item);
            }
        }

        private void LoadProjects()
        {
            var fileInfo = new FileInfo(ProjectsFilePath);

            if (fileInfo.Directory.Exists == false)
                fileInfo.Directory.Create();

            if (fileInfo.Exists == false)
            {
                fileInfo.Create().Close();
                return;
            }

            var projectPathes = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(fileInfo.FullName));

            if (projectPathes == null)
                return;

            foreach (var item in projectPathes)
            {
                try
                {

                var proj = new ProjectInfo(item);

                AddProject(proj);

                StaticInstances.ServerLogger.AppendInfo($"Project {proj.Info.Id} - {proj.Info.Name} loaded");
                }
                catch (Exception)
                {
                    StaticInstances.ServerLogger.AppendError($"Cannot load project {item}");
                }
            }
        }

        public void SaveProjLibrary()
        {
            File.WriteAllText(ProjectsFilePath, JsonConvert.SerializeObject(base.storage.Select(x => x.Value.ProjectDirPath)));
        }
    }
}
