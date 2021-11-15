using Cipher.RSA;
using Newtonsoft.Json;
using Publisher.Server.Info;
using Publisher.Server.Managers.Storages;
using Publisher.Server.Network;
using Publisher.Server.Network.PublisherClient;
using Publisher.Server.Network.PublisherClient.Packets.PacketRepository;
using ServerOptions.Extensions.Manager;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher.Server.Managers
{
    [ManagerLoad(1)]
    internal class ProjectsManager : ProjectsStorage
    {
        public static ProjectsManager Instance { get; private set; }

        public static string ProjectsFilePath => StaticInstances.ServerConfiguration.GetValue("paths.projects_library");

        public static string ProjectsBackupDirPath => StaticInstances.ServerConfiguration.GetValue("paths.projects_backup.dir");

        private FileSystemWatcher projectsLibraryWatcher;

        public ProjectsManager()
        {
            Instance = this;
            StaticInstances.ServerLogger.AppendInfo("Load projects");
            LoadProjects();
            LoadWatcher();
            StaticInstances.ServerLogger.AppendInfo("Load projects finished");
        }

        internal void SignIn(PublisherNetworkClient client, string project_id, string user_id, byte[] key)
        {
            //var session = StaticInstances.SessionManager.GetUser(user_id);

            //if (session != null && session.CurrentNetwork != null && session.CurrentNetwork.AliveState)
            //{
            //    Network.Packets.Project.SignInPacket.Send(client, Basic.SignStateEnum.AlreadyConnected);
            //    return;
            //}

            var proj = GetProject(project_id);

            if (proj == null)
            {
                ProjectPacketRepository.SendSignInResult(client, Basic.SignStateEnum.ProjectNotFound);
                return;
            }

            var user = proj.GetUser(user_id);

            if (user == null)
            {
                ProjectPacketRepository.SendSignInResult(client, Basic.SignStateEnum.UserNotFound);
                return;
            }

            if (user.CurrentNetwork != null && user.CurrentNetwork.AliveState && user.CurrentNetwork.Network.GetState())
            {
                ProjectPacketRepository.SendSignInResult(client, Basic.SignStateEnum.AlreadyConnected);
                return;
            }

            if (user.Cipher == null)
            {
                user.Cipher = new RSACipher();

                user.Cipher.LoadXml(user.RSAPrivateKey);
            }

            byte[] data = user.Cipher.Decode(key, 0, key.Length);

            if (Encoding.ASCII.GetString(data) == user_id)
            {
                client.UserInfo = user;
                user.CurrentNetwork = client;

                //StaticInstances.SessionManager.AddUser(client.UserInfo);

                client.RunAliveChecker();
                ProjectPacketRepository.SendSignInResult(client, Basic.SignStateEnum.Ok);

                proj.StartProcess(user.CurrentNetwork);
                return;
            }


            ProjectPacketRepository.SendSignInResult(client, Basic.SignStateEnum.UserNotFound);
        }

        private void LoadWatcher()
        {
            if (StaticInstances.CommandExecutor)
                return;

            var fi = new FileInfo(ProjectsFilePath);

            projectsLibraryWatcher = new FileSystemWatcher(fi.Directory.FullName, fi.Name);
            projectsLibraryWatcher.Deleted += DirectoryWatcher_Deleted;
            projectsLibraryWatcher.Changed += DirectoryWatcher_Changed;
            projectsLibraryWatcher.Created += DirectoryWatcher_Changed;
            projectsLibraryWatcher.EnableRaisingEvents = true;
        }

        private SemaphoreSlim projectsLibraryReadLocker = new SemaphoreSlim(1);

        private async void DirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed && e.ChangeType != WatcherChangeTypes.Created)
                return;

            await projectsLibraryReadLocker.WaitAsync();

            await Task.Delay(2_000);

            string json = null;

            try
            {
                StaticInstances.ServerLogger.AppendInfo($"{ProjectsFilePath} changed. Reloading");

                json = File.ReadAllText(e.FullPath);


                var projPathes = JsonConvert.DeserializeObject<string[]>(json);


                foreach (var item in storage.Where(x => !projPathes.Contains(x.Value.ProjectDirPath)))
                {
                    RemoveProject(item.Value);
                    StaticInstances.ServerLogger.AppendInfo($"Project {item.Value.Info.Name}({item.Value.Info.Id}) removed");
                }

                foreach (var item in projPathes)
                {
                    var exist = storage.Values.FirstOrDefault(x => x.ProjectDirPath == item);

                    if (exist == null)
                    {
                        exist = new ServerProjectInfo(item);
                        AddProject(exist);
                        StaticInstances.ServerLogger.AppendInfo($"Project {exist.Info.Name}({exist.Info.Id}) appended");
                    }
                }
                StaticInstances.ServerLogger.AppendInfo($"{ProjectsFilePath} changed. Success reloading");
            }
            catch (Exception ex) { StaticInstances.ServerLogger.AppendError(ex.ToString()); }
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

                    var proj = new ServerProjectInfo(item);

                    AddProject(proj);

                    StaticInstances.ServerLogger.AppendInfo($"Project {proj.Info.Id} - {proj.Info.Name} loaded");
                }
                catch (Exception ex)
                {
                    StaticInstances.ServerLogger.AppendError($"Cannot load project {item} {ex}");
                }
            }
        }

        public void SaveProjLibrary()
        {
            File.WriteAllText(ProjectsFilePath, JsonConvert.SerializeObject(storage.Select(x => x.Value.ProjectDirPath)));
        }
    }
}
