using Logger;
using Newtonsoft.Json;
using Publisher.Server.Configuration;
using Publisher.Server.Network;
using Publisher.Server.Network.Packets;
using Publisher.Server.Tools;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher.Server.Info
{
    public class ProjectInfoData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool FullReplace { get; set; }

        public bool Backup { get; set; }

        public List<string> IgnoreFilePaths { get; set; }
    }

    public class ProjectInfo
    {
        #region Path

        public string ProjectDirPath { get; private set; }

        public string PublisherDirPath => Path.Combine(ProjectDirPath, "Publisher");

        public string ProjectFilePath => Path.Combine(PublisherDirPath, "project.json");

        public string CacheFilePath => Path.Combine(PublisherDirPath, "cache.json");

        public string UsersDirPath => Path.Combine(PublisherDirPath, "users");

        public string UsersPublicksDirPath => Path.Combine(UsersDirPath, "publ");

        public string TempDirPath => Path.Combine(PublisherDirPath, "temp");

        public string LogsDirPath => Path.Combine(PublisherDirPath, "logs");

        public FileLogger CurrentLogger { get; set; } = null;

        #region Scripts

        public string ScriptsDirPath => Path.Combine(PublisherDirPath, "scripts");

        public string OnStartScriptPath => Path.Combine(ScriptsDirPath, "OnStart.ps1");

        public string OnEndScriptPath => Path.Combine(ScriptsDirPath, "OnEnd.ps1");

        public string OnFileStartScriptPath => Path.Combine(ScriptsDirPath, "OnFileStart.ps1");

        public string OnFileEndScriptPath => Path.Combine(ScriptsDirPath, "OnFileEnd.ps1");

        public string OnSuccessEndScriptPath => Path.Combine(ScriptsDirPath, "OnSuccessEnd.ps1");

        public string OnFailedScriptPath => Path.Combine(ScriptsDirPath, "OnFailedEnd.ps1");

        #endregion

        #endregion

        public bool StartProcess(UserInfo user)
        {
            user.CurrentProject = this;

            if (ProcessUser != null && ProcessUser != user)
            {
                if (!WaitQueue.Contains(user))
                    WaitQueue.Enqueue(user);

                if (ProcessUser.CurrentNetwork?.AliveState == true)
                {
                    return false;
                }
                else
                {
                    StopProcess(user.CurrentNetwork, false);
                }
            }
            runScriptOnStart();
            ProcessUser = user;

            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.ClientPackets.ProjectPublishStart);

            packet.WriteInt32(Info.IgnoreFilePaths.Count);

            foreach (var item in Info.IgnoreFilePaths)
            {
                packet.WriteString16(item);
            }

            user.CurrentNetwork.Send(packet);

            if (CurrentLogger != null)
            {
                CurrentLogger.Flush();
                CurrentLogger = null;
            }

            CurrentLogger = FileLogger.Initialize(LogsDirPath, user.Name);

            return true;
        }

        public void StopProcess(NetworkClient client, bool success, Dictionary<string,string> args = null)
        {
            if (ProcessUser == client.UserInfo)
            {
                if (client.CurrentFile != null)
                    EndFile(client);

                runScriptOnEnd();

                if (success)
                {
                    DumpFileList();
                    runScriptOnSuccessEnd(args ?? new Dictionary<string, string>());
                }
                else
                    runScriptOnFailedEnd();

                ProcessUser = null;

                while (WaitQueue.TryDequeue(out var newUser))
                {
                    if (newUser.CurrentNetwork?.AliveState == true)
                    {
                        StartProcess(newUser);
                        break;
                    }
                }
            }
        }

        private void ProcessFolder()
        {
            if (!File.Exists(CacheFilePath))
            {
                ProcessFolderData();
                DumpFileList();
            }
            else
            {
                string cacheJson = File.ReadAllText(CacheFilePath);

                if (!string.IsNullOrWhiteSpace(cacheJson))
                    FileInfoList = System.Text.Json.JsonSerializer.Deserialize<List<ProjectFileInfo>>(cacheJson);
                else
                    FileInfoList = new List<ProjectFileInfo>();

                var removed = new List<ProjectFileInfo>();

                foreach (var item in FileInfoList)
                {
                    item.FileInfo = new FileInfo(Path.Combine(ProjectDirPath, item.RelativePath));
                    item.Project = this;
                    if (!item.FileInfo.Exists)
                        removed.Add(item);
                }

                FileInfoList.RemoveAll(x => removed.Contains(x));
            }
        }

        public void ProcessFolderData()
        {
            var projDir = new DirectoryInfo(ProjectDirPath);

            var files = RecurciveFiles(projDir);

            var filePath = "";

            FileInfoList = new List<ProjectFileInfo>();

            foreach (var file in files)
            {
                filePath = file.FullName.Remove(0, ProjectDirPath.Length);
                if (Info.IgnoreFilePaths.Any(x => System.Text.RegularExpressions.Regex.IsMatch(filePath, x)))
                    continue;
                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);
            }
        }

        private void runScript(string fname, IEnumerable<KeyValuePair<string,object>> pms)
        {
            if (!File.Exists(fname))
                return;

            PowerShell ps = PowerShell.Create();
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Debug.DataAdded += Error_DataAdded;
            ps.Streams.Information.DataAdded += Error_DataAdded;
            ps.Streams.Progress.DataAdded += Error_DataAdded;
            ps.Streams.Verbose.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Error_DataAdded;


            ps.AddScript("Set-ExecutionPolicy Unrestricted");
            ps.AddCommand(fname);
            foreach (var item in pms)
            {
                ps.AddParameter(item.Key, item.Value);
            }


            var result = ps.Invoke();
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            if (sender is PSDataCollection<ProgressRecord> p)
            {
                BroadcastMessage($"Complete {p[e.Index].PercentComplete}%");
            }
            else if (sender is PSDataCollection<ErrorRecord> err)
            {
                BroadcastMessage(err[e.Index].ToString());
            }
            else if (sender is PSDataCollection<InformationRecord> info)
            {
                BroadcastMessage(info[e.Index].ToString());
            }
            else if (sender is PSDataCollection<VerboseRecord> verb)
            {
                BroadcastMessage(verb[e.Index].ToString());
            }
            else if (sender is PSDataCollection<WarningRecord> warn)
            {
                BroadcastMessage(warn[e.Index].ToString());
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            BroadcastMessage(e.Data);
        }

        public void BroadcastMessage(string log)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.ClientPackets.ServerLog);
            packet.WriteString16(log);

            foreach (var item in users)
            {
                item.CurrentNetwork?.Send(packet);
            }
        }

        private List<KeyValuePair<string, object>> GetAppendArgs(Dictionary<string, string> args, List<KeyValuePair<string,object>> args2)
        {
            foreach (var item in args)
            {
                args2.Add(new KeyValuePair<string, object>(item.Key, item.Value));
            }

            return args2;
        }

        private void runScriptOnStart() => runScript(OnStartScriptPath, new List<KeyValuePair<string, object>>() { 
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath) 
        });

        private void runScriptOnEnd() => runScript(OnEndScriptPath, new List<KeyValuePair<string, object>>() {
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath)
        });

        private void runScriptOnFileStart(string fullPath) => runScript(OnFileStartScriptPath, new List<KeyValuePair<string, object>>() {
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath),
          new KeyValuePair<string, object>("FilePath", fullPath)
        });


        private void runScriptOnFileEnd(string fullPath) => runScript(OnFileEndScriptPath, new List<KeyValuePair<string, object>>() {
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath),
          new KeyValuePair<string, object>("FilePath", fullPath)
        });

        internal void runScriptOnSuccessEnd(Dictionary<string,string> args) => runScript(OnSuccessEndScriptPath, GetAppendArgs(args, new List<KeyValuePair<string, object>>() {
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath)
        }));

        private void runScriptOnFailedEnd() => runScript(OnFailedScriptPath, new List<KeyValuePair<string, object>>() {
          new KeyValuePair<string, object>("CurrentDir", ProjectDirPath)
        });

        private IEnumerable<FileInfo> RecurciveFiles(DirectoryInfo di)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (var item in di.GetFiles())
            {
                files.Add(item);
            }

            foreach (var item in di.GetDirectories())
            {
                files.AddRange(RecurciveFiles(item));
            }

            return files;
        }

        internal UserInfo GetUser(string userId)
        {
            return users.FirstOrDefault(x => x.Id == userId);
        }

        internal bool AddUser(UserInfo user)
        {
            if (users.Any(x => string.Compare(x.Name, user.Name, true) > -1))
            {
                StaticInstances.ServerLogger.AppendError($"{user.Name} already exist in project {Info.Name}");
                return false;
            }

            File.WriteAllText(Path.Combine(UsersPublicksDirPath, $"{user.Name}_{user.Id}.pubuk"), JsonConvert.SerializeObject(new
            {
                user.Id,
                user.Name,
                user.RSAPublicKey
            }));

            File.WriteAllText(Path.Combine(UsersDirPath, $"{user.Name}_{user.Id}.priuk"), JsonConvert.SerializeObject(new
            {
                user.Id,
                user.Name,
                user.RSAPublicKey,
                user.PSAPrivateKey
            }));

            return true;
        }

        public bool RemoveFile(string path)
        {
            var file = FileInfoList.FirstOrDefault(x => x.RelativePath == path);

            if (file == null)
                return true;
            try
            {
                File.Delete(file.Path);
                FileInfoList.Remove(file);
                return true;
            }
            catch
            {
            }

            return false;
        }

        public void DumpFileList()
        {
            File.WriteAllText(CacheFilePath, System.Text.Json.JsonSerializer.Serialize(FileInfoList.Select(x=>new { x.RelativePath, x.Hash })));
        }

        internal void Reload(ProjectInfoData info)
        {
            Info = info;

            if (info.IgnoreFilePaths == null)
                info.IgnoreFilePaths = new List<string>();


            if (!info.IgnoreFilePaths.Contains(Path.Combine("Publisher", "*")))
                info.IgnoreFilePaths.Add(Path.Combine("Publisher", "*"));

            ProcessFolder();
        }

        private void CreateWatchers()
        {
            UsersWatch = new FileSystemWatcher(UsersDirPath, "*.priuk");
            UsersWatch.Changed += UsersWatch_Changed; 
            UsersWatch.Deleted += UsersWatch_Deleted;
            UsersWatch.EnableRaisingEvents = true;
        }

        private void UsersWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            users.RemoveAll(x => x.FileName == e.FullPath);
        }

        private void UsersWatch_Changed(object sender, FileSystemEventArgs e)
        {
            UserInfo user = null;
            try { user = new UserInfo(e.FullPath); } catch { return; }

            AddOrUpdateUser(user);
        }

        private void AddOrUpdateUser(UserInfo user)
        {
            var exist = users.Find(x => x.Id == user.Id);

            if (exist == null)
            {
                users.Add(user);
            }
            else
            {
                exist.Reload(user);
            }
        }

        internal void StartFile(NetworkClient client, string relativePath)
        {
            EndFile(client);

            client.CurrentFile = FileInfoList.FirstOrDefault(x => x.RelativePath == relativePath);

            if (client.CurrentFile != null && !client.CurrentFile.FileInfo.Exists)
            {
                FileInfoList.Remove(client.CurrentFile);
                client.CurrentFile = null;
            }

            if (client.CurrentFile == null)
            {
                client.CurrentFile = new ProjectFileInfo(ProjectDirPath, new FileInfo(Path.Combine(ProjectDirPath, relativePath)),this);
            }

            runScriptOnFileStart(client.CurrentFile.Path);

            client.CurrentFile.StartFile();
        }

        internal void EndFile(NetworkClient client)
        {
            if (client.CurrentFile == null)
                return;

            client.CurrentFile.EndFile();

            runScriptOnFileEnd(client.CurrentFile.Path);

            if (!FileInfoList.Contains(client.CurrentFile))
                FileInfoList.Add(client.CurrentFile);
            client.CurrentFile = null;
        }

        public ProjectInfoData Info { get; private set; }

        public List<ProjectFileInfo> FileInfoList { get; private set; }

        public AutoResetEvent ProcessLocked { get; private set; }

        public UserInfo ProcessUser { get; private set; }

        public ConcurrentQueue<UserInfo> WaitQueue { get; set; }


        private readonly List<UserInfo> users = new List<UserInfo>();

        public FileSystemWatcher UsersWatch;

        private void CreateDefault()
        {
            if (!Directory.Exists(PublisherDirPath))
                Directory.CreateDirectory(PublisherDirPath);

            if (!Directory.Exists(UsersDirPath))
                Directory.CreateDirectory(UsersDirPath);

            if (!Directory.Exists(UsersPublicksDirPath))
                Directory.CreateDirectory(UsersPublicksDirPath);

            if (!Directory.Exists(TempDirPath))
                Directory.CreateDirectory(TempDirPath);

            if (!Directory.Exists(ScriptsDirPath))
                Directory.CreateDirectory(ScriptsDirPath);

            if (!Directory.Exists(LogsDirPath))
                Directory.CreateDirectory(LogsDirPath);
        }

        private void LoadUsers()
        {
            foreach (var item in Directory.GetFiles(UsersDirPath, "*.priuk"))
            {
                try
                {
                    AddOrUpdateUser(new UserInfo(item));
                }
                catch (Exception ex)
                {
                    StaticInstances.ServerLogger.AppendError($"cannot read {item} user data, exception: {ex.ToString()}");
                }
            }
        }

        public ProjectInfo(string projectPath)
        {
            ProjectDirPath = projectPath;

            CreateDefault();
            CreateWatchers();
            LoadUsers();

            ProcessLocked = new AutoResetEvent(true);
            WaitQueue = new ConcurrentQueue<UserInfo>();

            string json = File.ReadAllText(ProjectFilePath);
            Reload(JsonConvert.DeserializeObject<ProjectInfoData>(json));
        }

        public ProjectInfo(ProjectInfoData pid)
        {
            Reload(pid);
        }

        public ProjectInfo(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("project creating");
            Info = new ProjectInfoData()
            {
                Id = Guid.NewGuid().ToString(),
                Name = args["name"],
                FullReplace = args.ContainsKey("full_replace") && Convert.ToBoolean(args["full_replace"]),
                Backup = args.ContainsKey("backup") && Convert.ToBoolean(args["backup"]),
                IgnoreFilePaths = new List<string>() { Path.Combine("Publisher", "*") }
            };

            ProjectDirPath = args["directory"];

            CreateDefault();

            File.WriteAllText(ProjectFilePath, JsonConvert.SerializeObject(Info));
        }
    }
}
