using SCLogger;
using Newtonsoft.Json;
using Publisher.Server.Network;
using SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Publisher.Basic;
using Publisher.Server.Managers;
using System.Threading.Tasks;
using SCL;
using Publisher.Server.Network.ClientPatchPackets;
using Cipher.RSA;
using System.Text;
using System.Text.RegularExpressions;
using Publisher.Server.Info.PacketInfo;
using SocketCore.Utils;
using System.Reflection;
using Publisher.Server.Network.PublisherClient;
using Publisher.Server.Network.PublisherClient.Packets;

namespace Publisher.Server.Info
{
    public partial class ServerProjectInfo : IDisposable
    {
        #region Path

        public string ProjectDirPath { get; private set; }

        public string PublisherDirPath => Path.Combine(ProjectDirPath, "Publisher");

        public string ProjectBackupPath => Path.Combine(PublisherDirPath, "Backup");

        public string ProjectFilePath => Path.Combine(PublisherDirPath, "project.json");

        public string CacheFilePath => Path.Combine(PublisherDirPath, "cache.json");

        public string UsersDirPath => Path.Combine(PublisherDirPath, "users");

        public string UsersPublicksDirPath => Path.Combine(UsersDirPath, "publ");

        public string TempDirPath => Path.Combine(PublisherDirPath, "temp");

        public string LogsDirPath => Path.Combine(PublisherDirPath, "logs");


        #region Scripts

        public string ScriptsDirPath => Path.Combine(PublisherDirPath, "scripts");

        public string OnStartScriptPath => Path.Combine(ScriptsDirPath, "OnStart.cs");

        public string OnEndScriptPath => Path.Combine(ScriptsDirPath, "OnEnd.cs");

        public string OnFileStartScriptPath => Path.Combine(ScriptsDirPath, "OnFileStart.cs");

        public string OnFileEndScriptPath => Path.Combine(ScriptsDirPath, "OnFileEnd.cs");

        public string OnSuccessEndScriptPath => Path.Combine(ScriptsDirPath, "OnSuccessEnd.cs");

        public string OnFailedScriptPath => Path.Combine(ScriptsDirPath, "OnFailedEnd.cs");

        //public string OnStartScriptPath => Path.Combine(ScriptsDirPath, "OnStart.ps1");

        //public string OnEndScriptPath => Path.Combine(ScriptsDirPath, "OnEnd.ps1");

        //public string OnFileStartScriptPath => Path.Combine(ScriptsDirPath, "OnFileStart.ps1");

        //public string OnFileEndScriptPath => Path.Combine(ScriptsDirPath, "OnFileEnd.ps1");

        //public string OnSuccessEndScriptPath => Path.Combine(ScriptsDirPath, "OnSuccessEnd.ps1");

        //public string OnFailedScriptPath => Path.Combine(ScriptsDirPath, "OnFailedEnd.ps1");

        #endregion

        #endregion

        #region Scripts

        private void CheckScriptsExists(NetScript.Script script = null)
        {
            if (File.Exists(Path.Combine(ScriptsDirPath, "ScriptCore.cs")) == false)
                File.WriteAllText(Path.Combine(ScriptsDirPath, "ScriptCore.cs"), (script ?? new NetScript.Script()).DumpCoreCode());

            if (File.Exists(OnStartScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();
                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnStart() {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnStartScriptPath, scriptContent.ToString());
            }

            if (File.Exists(OnEndScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();
                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnEnd() {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnEndScriptPath, scriptContent.ToString());
            }

            if (File.Exists(OnFileStartScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();

                scriptContent.AppendLine("using System.Collections;");
                scriptContent.AppendLine("using System.Collections.Generic;");

                scriptContent.AppendLine();

                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnFileStart(Dictionary<string, object> args) {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnFileStartScriptPath, scriptContent.ToString());
            }

            if (File.Exists(OnFileEndScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();

                scriptContent.AppendLine("using System.Collections;");
                scriptContent.AppendLine("using System.Collections.Generic;");

                scriptContent.AppendLine();

                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnFileEnd(Dictionary<string, object> args) {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnFileEndScriptPath, scriptContent.ToString());
            }

            if (File.Exists(OnSuccessEndScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();

                scriptContent.AppendLine("using System.Collections;");
                scriptContent.AppendLine("using System.Collections.Generic;");

                scriptContent.AppendLine();

                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnSuccessEnd(Dictionary<string, object> args) {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnSuccessEndScriptPath, scriptContent.ToString());
            }

            if (File.Exists(OnFailedScriptPath) == false)
            {
                StringBuilder scriptContent = new StringBuilder();
                scriptContent.AppendLine("public partial class PublisherScript {");

                scriptContent.AppendLine("\tpublic static void OnFailed() {");
                scriptContent.AppendLine("\t}");

                scriptContent.AppendLine("}");

                File.WriteAllText(OnFailedScriptPath, scriptContent.ToString());
            }
        }

        NetScript.Script script;

        DateTime? scriptLatestBuilded;

        DateTime scriptLatestChanged = DateTime.UtcNow;

        private MethodInfo OnStartMethod;

        private MethodInfo OnEndMethod;

        private MethodInfo OnFileStartMethod;

        private MethodInfo OnFileEndMethod;

        private MethodInfo OnSuccessEndMethod;

        private MethodInfo OnFailedMethod;

        private NetScript.Script getScript(bool force = false)
        {
            if (scriptLatestBuilded.HasValue && scriptLatestBuilded >= scriptLatestChanged && force == false)
                return script;

            //if (script != null)
            //    script.Disponse();
            //try
            //{
            script = new NetScript.Script();

            script.RegisterExecutableReference();

            script.RegisterCoreReference("System.dll");
            script.RegisterCoreReference("System.IO.dll");
            script.RegisterCoreReference("System.IO.FileSystem.dll");
            script.RegisterCoreReference("System.IO.FileSystem.Primitives.dll");
            script.RegisterCoreReference("System.Linq.dll");
            script.RegisterCoreReference("System.Collections.dll");
            script.RegisterCoreReference("System.ComponentModel.Primitives.dll");
            script.RegisterCoreReference("System.Diagnostics.Process.dll");

            script.RegistrationGlobalVariable(new NetScript.GlobalVariable("CurrentProject", typeof(ServerProjectInfo)));

            CheckScriptsExists(script);

            script.AddFolder(ScriptsDirPath, true);

            script.Compile();

            script.SetGlobalVariable("CurrentProject", this);

            scriptLatestBuilded = DateTime.UtcNow;

            OnStartMethod = script.GetMethod("PublisherScript", "OnStart");

            OnEndMethod = script.GetMethod("PublisherScript", "OnEnd");

            OnFileStartMethod = script.GetMethod("PublisherScript", "OnFileStart");

            OnFileEndMethod = script.GetMethod("PublisherScript", "OnFileEnd");

            OnSuccessEndMethod = script.GetMethod("PublisherScript", "OnSuccessEnd");

            OnFailedMethod = script.GetMethod("PublisherScript", "OnFailed");
            //}
            //catch (Exception ex)
            //{
            //    BroadcastMessage(ex.ToString());
            //    if (ProcessUser != null)
            //    {
            //        StopProcess(ProcessUser.CurrentNetwork, false);
            //    }
            //}

            return script;
        }

        internal void CheckScripts()
        {
            getScript();
        }

        private void runScript(Func<MethodInfo> function, Dictionary<string, object> pms)
        {
            if (Info.PreventScriptExecution)
                return;

            if (pms == null)
                getScript().InvokeMethod<object>(method: function(), _obj: null, args: null);
            else
                getScript().InvokeMethod<object>(method: function(), _obj: null, args: pms);
            //getScript().InvokeMethod("PublisherScript", functionName, null, pms);
        }

        private void runScriptOnStart() => runScript(() => OnStartMethod, null);

        private void runScriptOnEnd() => runScript(() => OnEndMethod, null);

        private void runScriptOnFileStart(string fullPath) => runScript(() => OnFileStartMethod, new Dictionary<string, object>() {
            { "FilePath", fullPath }
        });

        private void runScriptOnFileEnd(string fullPath) => runScript(() => OnFileEndMethod, new Dictionary<string, object>() {
            { "FilePath", fullPath }
        });

        internal void runScriptOnSuccessEnd(Dictionary<string, string> args) => runScript(() => OnSuccessEndMethod, args.ToDictionary(x => x.Key, x => (object)x.Value));

        private void runScriptOnFailedEnd() => runScript(() => OnFailedMethod, null);

        public void BroadcastMessage(string log)
        {
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(PublisherClientPackets.ServerLog);
            packet.WriteString16(log);

            CurrentLogger?.AppendLog(log);

            foreach (var item in users)
            {
                item.CurrentNetwork?.Send(packet);
            }
        }

        #endregion

        #region Watchers

        private void CreateWatchers()
        {
            UsersWatch = new FileSystemWatcher(UsersDirPath, "*.priuk");
            UsersWatch.Changed += UsersWatch_Changed;
            UsersWatch.Deleted += UsersWatch_Deleted;
            UsersWatch.EnableRaisingEvents = true;

            SettingsWatch = new FileSystemWatcher(PublisherDirPath, new FileInfo(ProjectFilePath).Name);

            SettingsWatch.Changed += SettingsWatch_Changed;
            SettingsWatch.Deleted += SettingsWatch_Deleted;
            SettingsWatch.EnableRaisingEvents = true;

            ScriptsWatch = new FileSystemWatcher(ScriptsDirPath, "*.cs");

            ScriptsWatch.Created += ScriptsWatch_Changed;
            ScriptsWatch.Changed += ScriptsWatch_Changed;
            ScriptsWatch.Deleted += ScriptsWatch_Changed;
            ScriptsWatch.EnableRaisingEvents = true;
        }

        private async void ScriptsWatch_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(1_500);
            scriptLatestChanged = DateTime.UtcNow;
        }

        private void SettingsWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            ProjectsManager.Instance.RemoveProject(this);
        }

        private async void SettingsWatch_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(1500);
            var oldInfo = Info;
            try
            {
                LoadProjectInfo();
            }
            catch { return; }

            StaticInstances.ServerLogger.AppendInfo($"{ProjectFilePath} changed \r\nold {JsonConvert.SerializeObject(oldInfo)}\r\nnew {JsonConvert.SerializeObject(Info)}");


            if (oldInfo.PatchInfo == null && Info.PatchInfo != null)
            {
                if (PatchClient == null)
                    LoadPatch();
            }
            else if (oldInfo.PatchInfo != null && Info.PatchInfo == null)
            {
                if (PatchClient != null)
                {
                    PatchClient.SignOutProject(this);
                    PatchClient = null;
                }
            }
            else if (oldInfo.PatchInfo != null && Info.PatchInfo != null &&
                (oldInfo.PatchInfo.IpAddress != Info.PatchInfo.IpAddress ||
                oldInfo.PatchInfo.Port != Info.PatchInfo.Port ||
                oldInfo.PatchInfo.SignName != Info.PatchInfo.SignName ||
                oldInfo.PatchInfo.InputCipherKey != Info.PatchInfo.InputCipherKey ||
                oldInfo.PatchInfo.OutputCipherKey != Info.PatchInfo.OutputCipherKey))
            {
                if (PatchClient != null)
                    PatchClient.SignOutProject(this);
                LoadPatch();
            }
        }

        private void UsersWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            users.RemoveAll(x => x.FileName == e.FullPath);
        }

        private async void UsersWatch_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(1500);
            try
            {
                var user = new UserInfo(e.FullPath);
                AddOrUpdateUser(user);
            }
            catch { }
        }

        #endregion

        #region Temp

        private void initializeTemp()
        {
            var di = new DirectoryInfo(TempDirPath);

            if (di.Exists)
                di.Delete(true);

            di.Create();
        }

        private bool processTemp()
        {
            initializeBackup();
            foreach (var item in processFileList)
            {
                addBackupFile(item);

                runScriptOnFileStart(item.Path);

                if (item.TempRelease() == false)
                {
                    BroadcastMessage($"Error!! cannot move file from temp {item.RelativePath}!!");
                    return false;
                }

                runScriptOnFileEnd(item.Path);
            }

            return true;
        }

        #endregion

        #region Backup

        private string currentBackupDirPath = "";

        private void initializeBackup()
        {
            if (Info.Backup == false)
                return;

            currentBackupDirPath = Path.Combine(ProjectBackupPath, DateTime.UtcNow.ToString("yyyy-MM-ddTHH_mm_ss"));
        }

        private void addBackupFile(ProjectFileInfo file)
        {
            if (Info.Backup == false)
                return;

            if (file.FileInfo.Exists == false)
                return;

            var fi = new FileInfo(Path.Combine(currentBackupDirPath, file.RelativePath));

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            file.FileInfo.CopyTo(fi.FullName);
        }

        private bool recoveryBackup()
        {
            if (Info.Backup == false)
                return false;
            var dir = new DirectoryInfo(currentBackupDirPath);

            if (dir.Exists == false)
                return true;

            foreach (var item in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                item.CopyTo(Path.Combine(ProjectDirPath, Path.GetRelativePath(dir.FullName, item.FullName)), true);
            }

            dir.Delete(true);

            currentBackupDirPath = string.Empty;

            return true;
        }

        #endregion

        #region Logger

        public FileLogger CurrentLogger { get; set; } = null;

        private void initializeLogger()
        {
            closeLogger();

            var uid = ProcessUser?.Id ?? currentDownloader?.UserInfo?.Id ?? "Unknown";

            CurrentLogger = FileLogger.Initialize(LogsDirPath, $"upload {DateTime.Now:yyyy-MM-dd_HH.mm.ss} - {uid}");
        }

        private void closeLogger()
        {
            if (CurrentLogger != null)
            {
                CurrentLogger.Flush();
                CurrentLogger.Dispose();
            }
        }

        #endregion

        #region Publish

        public bool StartProcess(PublisherNetworkClient client)
        {
            client.UserInfo.CurrentProject = this;

            patchLocker.WaitOne();

            if (ProcessUser != null && ProcessUser != client.UserInfo)
            {
                if (!WaitQueue.Contains(client.UserInfo))
                    WaitQueue.Enqueue(client.UserInfo);

                if (ProcessUser.CurrentNetwork?.AliveState == true && ProcessUser.CurrentNetwork?.Network?.GetState() == true)
                {
                    return false;
                }
                else
                {
                    if (StopProcess(client.UserInfo.CurrentNetwork, false))
                        patchLocker.WaitOne();
                }
            }

            ProcessUser = client.UserInfo;

            initializeLogger();
            initializeTemp();

            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PublisherClientPackets.ProjectPublishStart);

            packet.WriteInt32(Info.IgnoreFilePaths.Count);

            foreach (var item in Info.IgnoreFilePaths)
            {
                packet.WriteString16(item);
            }

            client.UserInfo.CurrentNetwork.Send(packet);

            return true;
        }

        private List<ProjectFileInfo> processFileList = new List<ProjectFileInfo>();

        public bool StopProcess(PublisherNetworkClient client, bool success, Dictionary<string, string> args = null)
        {
            if (ProcessUser == client.UserInfo)
            {
                if (client.CurrentFile != null)
                    EndFile(client);

                if (success)
                {
                    try { runScriptOnStart(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); success = false; }

                    if (success)
                    {
                        try
                        {
                            success = processTemp();
                        }
                        catch (Exception ex)
                        {
                            success = false;
                            BroadcastMessage(ex.ToString());
                        }

                        if (!success)
                        {
                            recoveryBackup();
                            ProcessFolder();
                        }
                    }

                    try { runScriptOnEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                }

                processFileList.Clear();

                if (success)
                {
                    DumpFileList();

                    try { runScriptOnSuccessEnd(args ?? new Dictionary<string, string>()); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                    Info.LatestUpdate = DateTime.UtcNow;
                    
                    SaveProjectInfo();

                    broadcastUpdateTime();
                }
                else
                    try { runScriptOnFailedEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                ProcessUser = null;
                
                patchLocker.Set();

                while (WaitQueue.TryDequeue(out var newUser))
                {
                    if (newUser.CurrentNetwork?.AliveState == true && newUser.CurrentNetwork?.Network?.GetState() == true)
                    {
                        StartProcess(newUser.CurrentNetwork);
                        break;
                    }
                }
                return true;
            }

            return false;
        }

        internal void StartFile(IProcessFileContainer client, string relativePath, DateTime createTime, DateTime updateTime)
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
                client.CurrentFile = new ProjectFileInfo(ProjectDirPath, new FileInfo(Path.Combine(ProjectDirPath, relativePath)), this);
            }

            processFileList.Add(client.CurrentFile);
            client.CurrentFile.StartFile(createTime,updateTime);
        }

        internal void EndFile(IProcessFileContainer client)
        {
            if (client.CurrentFile == null)
                return;

            if (!FileInfoList.Contains(client.CurrentFile))
                FileInfoList.Add(client.CurrentFile);

            client.CurrentFile.EndFile();

            client.CurrentFile = null;
        }

        #endregion

        #region Files

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

                StaticInstances.ServerLogger.AppendInfo($"Project {Info.Name}({Info.Id}) Loaded {FileInfoList.Count} files");

                var removed = new List<ProjectFileInfo>();

                foreach (var item in FileInfoList)
                {
                    item.FileInfo = new FileInfo(Path.Combine(ProjectDirPath, item.RelativePath));
                    item.Project = this;
                    if (!item.FileInfo.Exists)
                        removed.Add(item);
                }

                StaticInstances.ServerLogger.AppendInfo($"Project {Info.Name}({Info.Id}) Removed {FileInfoList.RemoveAll(x => removed.Contains(x))} invalid files");

                GC.Collect();
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
                if (Info.IgnoreFilePaths.Any(x => Regex.IsMatch(filePath, x)))
                    continue;
                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);
            }
        }

        public void DumpFileList()
        {
            File.WriteAllText(CacheFilePath, System.Text.Json.JsonSerializer.Serialize(FileInfoList.Select(x => new { x.RelativePath, x.Hash })));
        }

        internal void ReIndexing()
        {
            StaticInstances.ServerLogger.AppendInfo($"Try reindexing project {Info.Name}({Info.Id})");

            ProcessFolder();

            string oldHash = default;
            bool exists = false;

            foreach (var item in FileInfoList)
            {
                oldHash = item.Hash;

                item.CalculateHash();

                if (oldHash != item.Hash)
                {
                    StaticInstances.ServerLogger.AppendInfo($"{item.RelativePath} invaliid hash ({oldHash} vs {item.Hash})");
                    exists = true;
                }
            }

            var di = new DirectoryInfo(ProjectDirPath);

            string filePath = "";

            foreach (var file in RecurciveFiles(di))
            {
                filePath = Path.GetRelativePath(ProjectDirPath, file.FullName);

                if (Info.IgnoreFilePaths.Any(x => Regex.IsMatch(filePath, x)))
                    continue;

                if (FileInfoList.Any(x => x.RelativePath == filePath))
                    continue;

                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);

                StaticInstances.ServerLogger.AppendInfo($"{pfi.RelativePath} new file {pfi.Hash}");
            }

            if (exists)
            {
                DumpFileList();
                Info.LatestUpdate = DateTime.UtcNow;
                SaveProjectInfo();
                StaticInstances.ServerLogger.AppendInfo($"Success reindexing project");
            }

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
            catch (Exception ex)
            {
                StaticInstances.ServerLogger.AppendError(ex.ToString());
            }

            return false;
        }

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

        #endregion

        #region Users

        internal UserInfo GetUser(string userId)
        {
            return users.FirstOrDefault(x => x.Id == userId);
        }

        internal bool AddUser(UserInfo user)
        {
            if (users.Any(x => x.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
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
                user.RSAPrivateKey
            }));

            return true;
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

        #endregion

        internal void Reload(ProjectInfoData info)
        {
            Info = info;

            if (info.IgnoreFilePaths == null)
                info.IgnoreFilePaths = new List<string>();

            if (!info.IgnoreFilePaths.Contains(Path.Combine("Publisher", "[\\s|\\S]")))
                info.IgnoreFilePaths.Add(Path.Combine("Publisher", "[\\s|\\S]"));

            if (info.IgnoreFilePaths.RemoveAll(x => x.Contains("**")) > 0)
                SaveProjectInfo();

            ProcessFolder();
        }

        public ProjectInfoData Info { get; private set; }

        public List<ProjectFileInfo> FileInfoList { get; private set; }

        public UserInfo ProcessUser { get; private set; }

        public ConcurrentQueue<UserInfo> WaitQueue { get; set; }


        private readonly List<UserInfo> users = new List<UserInfo>();

        public FileSystemWatcher UsersWatch;

        public FileSystemWatcher SettingsWatch;

        public FileSystemWatcher ScriptsWatch;

        private void CreateDefault()
        {
            if (!Directory.Exists(PublisherDirPath))
            {
                Directory.CreateDirectory(PublisherDirPath); 

                if (Directory.Exists("Template"))
                    DirectoryCopy("Template", PublisherDirPath, true);
            }

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

            

            CheckScriptsExists(script);
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

        public ServerProjectInfo(string projectPath)
        {
            ProjectDirPath = projectPath;

            CreateDefault();
            CreateWatchers();
            LoadUsers();

            WaitQueue = new ConcurrentQueue<UserInfo>();

            LoadProjectInfo();

            LoadPatch();
        }

        public ServerProjectInfo(ProjectInfoData pid)
        {
            Reload(pid);
        }

        public ServerProjectInfo(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("project creating");
            Info = new ProjectInfoData()
            {
                Id = args.ContainsKey("project_id") ? args["project_id"] : Guid.NewGuid().ToString(),
                Name = args["name"],
                FullReplace = args.ContainsKey("full_replace") && Convert.ToBoolean(args["full_replace"]),
                Backup = args.ContainsKey("backup") && Convert.ToBoolean(args["backup"]),
                IgnoreFilePaths = new List<string>() { Path.Combine("Publisher", "[\\s|\\S]") }
            };

            ProjectDirPath = args["directory"];

            CreateDefault();

            SaveProjectInfo();
        }

        public void SaveProjectInfo()
        {
            File.WriteAllText(ProjectFilePath, JsonConvert.SerializeObject(Info));
        }

        public void LoadProjectInfo()
        {
            string json = File.ReadAllText(ProjectFilePath);
            Reload(JsonConvert.DeserializeObject<ProjectInfoData>(json));
        }

        public void UpdatePatchInfo(ProjectPatchInfo info)
        {
            Info.PatchInfo = info;
            SaveProjectInfo();
        }

        public void Dispose()
        {
            ScriptsWatch.Dispose();
            SettingsWatch.Dispose();
            UsersWatch.Dispose();

            if (PatchClient != null)
                PatchClient.SignOutProject(this);
        }



        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
