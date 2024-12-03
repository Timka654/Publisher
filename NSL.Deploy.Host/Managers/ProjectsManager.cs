﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSL.Cipher.RSA;
using NSL.Logger;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Dev.Test.Utils;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Managers.Storages;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Managers
{
    [ManagerLoad(1)]
    internal class ProjectsManager : ProjectsStorage
    {
        public static ProjectsManager Instance { get; private set; }

        public static string ProjectsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.LibraryFilePath);

        public static string GlobalBothUsersFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.GlobalBothUsersFolderPath);

        public static string GlobalPublishUsersFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.GlobalPublishUsersFolderPath);

        public static string GlobalProxyUsersFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.GlobalProxyUsersFolderPath);

        public UserStorage GlobalBothUserStorage { get; private set; }
        public UserStorage GlobalPublishUserStorage { get; private set; }
        public UserStorage GlobalProxyUserStorage { get; private set; }

        private FSWatcher projectsLibraryWatcher;

        public ProjectsManager()
        {
            Instance = this;
            PublisherServer.ServerLogger.AppendInfo("Load projects");

            GlobalBothUserStorage = new UserStorage(GlobalBothUsersFolderPath, "*.priuk|*.pubuk");
            GlobalPublishUserStorage = new UserStorage(GlobalPublishUsersFolderPath);
            GlobalProxyUserStorage = new UserStorage(GlobalProxyUsersFolderPath, "*.pubuk");

            LoadProjects();
            LoadWatcher();
            PublisherServer.ServerLogger.AppendInfo("Load projects finished");
        }

        public UserInfo? GetProxyUser(string signName)
        {
            return GlobalProxyUserStorage.GetUser(signName) ?? GlobalBothUserStorage.GetUser(signName);
        }

        internal SignStateEnum SignIn(PublisherNetworkClient client, PublishSignInRequestModel request)
        {
            var proj = GetProject(request.ProjectId);

            if (proj == null) return SignStateEnum.ProjectNotFound;


            var user = proj.GetUser(request.UserId) ?? GlobalBothUserStorage.GetUser(request.UserId) ?? GlobalPublishUserStorage.GetUser(request.UserId);

            if (user == null) return SignStateEnum.UserNotFound;

            if (user.Cipher == null)
            {
                user.Cipher = new RSACipher();

                user.Cipher.LoadXml(user.RSAPrivateKey);
            }

            byte[] data = user.Cipher.Decode(request.IdentityKey, 0, request.IdentityKey.Length);

            if (Encoding.ASCII.GetString(data) == request.UserId)
            {
                client.UserInfo = user;

                client.PublishContext = new ProjectPublishContext()
                {
                    ProjectInfo = proj,
                    Network = client,
                    Actual = false,
                    Platform = request.OSType,
                    UploadMethod = request.UploadMethod
                };

                proj.StartPublishProcess(client);


                return SignStateEnum.Ok;
            }

            return SignStateEnum.InvalidIdentityKey;
        }

        private void LoadWatcher()
        {
            if (PublisherServer.CommandExecutor)
                return;

            var fi = new FileInfo(ProjectsFilePath);

            projectsLibraryWatcher = new FSWatcher(fi.Directory.GetNormalizedDirectoryPath(), fi.Name
                , onDeleted: DirectoryWatcher_Deleted
                , onChanged: DirectoryWatcher_Changed
                , onCreated: DirectoryWatcher_Changed);
        }

        private void DirectoryWatcher_Changed(FileSystemEventArgs e)
        {
            string json = null;

            PublisherServer.ServerLogger.AppendInfo($"{ProjectsFilePath} changed. Reloading");

            json = File.ReadAllText(e.FullPath);


            var projPathes = JsonConvert.DeserializeObject<string[]>(json);


            foreach (var item in storage.Where(x => !projPathes.Contains(x.Value.ProjectDirPath)))
            {
                RemoveProject(item.Value);
                PublisherServer.ServerLogger.AppendInfo($"Project {item.Value.Info.Name}({item.Value.Info.Id}) removed");
            }

            foreach (var item in projPathes)
            {
                var exist = storage.Values.FirstOrDefault(x => x.ProjectDirPath == item);

                if (exist == null)
                {
                    exist = new ServerProjectInfo(item);
                    AddProject(exist);
                    PublisherServer.ServerLogger.AppendInfo($"Project {exist.Info.Name}({exist.Info.Id}) appended");
                }
            }
            PublisherServer.ServerLogger.AppendInfo($"{ProjectsFilePath} changed. Success reloading");
        }

        private void DirectoryWatcher_Deleted(FileSystemEventArgs e)
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

            var projectPathes = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(fileInfo.GetNormalizedFilePath()));

            if (projectPathes == null)
                return;

            foreach (var item in projectPathes)
            {
                try
                {
                    if (!Directory.Exists(Path.Combine(item, "Publisher")))
                    {
                        PublisherServer.ServerLogger.AppendError($"Have invalid project path - {item}. No exists Publisher dir");
                        
                        continue;
                    }

                    if (!File.Exists(Path.Combine(item, "Publisher", "project.json")))
                    {
                        PublisherServer.ServerLogger.AppendError($"Have invalid project path - {item}. No exists project.json file");

                        continue;
                    }

                    var proj = new ServerProjectInfo(item);

                    AddProject(proj);

                    PublisherServer.ServerLogger.AppendInfo($"Project {proj.Info.Id} - {proj.Info.Name} loaded");
                }
                catch (Exception ex)
                {
                    PublisherServer.ServerLogger.AppendError($"Cannot load project {item} {ex}");
                }
            }
        }

        public void SaveProjLibrary()
        {
            File.WriteAllText(ProjectsFilePath, JsonConvert.SerializeObject(storage.Select(x => x.Value.ProjectDirPath)));
        }

        #region Storages

        public new bool AddProject(ServerProjectInfo project)
        {
            var result = base.AddProject(project);

            //if (result)
            //    PublisherServer.ServiceManager.TryRegisterService(project);

            return result;
        }

        public bool RemoveProject(ServerProjectInfo project, bool fullRemove = true)
        {
            var result = base.RemoveProject(project);

            //if (result && fullRemove)
            //    PublisherServer.ServiceManager.UnregisterService(project);

            return result;
        }

        #endregion
    }
}