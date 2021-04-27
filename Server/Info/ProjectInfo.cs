using SCLogger;
using Newtonsoft.Json;
using Publisher.Server.Network;
using Publisher.Server.Network.Packets;
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

namespace Publisher.Server.Info
{
    public class ProjectInfo
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

        public FileLogger CurrentLogger { get; set; } = null;

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

            script.RegistrationGlobalVariable(new NetScript.GlobalVariable("CurrentProject", typeof(ProjectInfo)));

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
            packet.SetPacketId(Basic.PublisherClientPackets.ServerLog);
            packet.WriteString16(log);

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

        #region Publish

        public bool StartProcess(UserInfo user)
        {
            user.CurrentProject = this;

            patchLocker.WaitOne();

            if (ProcessUser != null && ProcessUser != user)
            {
                if (!WaitQueue.Contains(user))
                    WaitQueue.Enqueue(user);

                if (ProcessUser.CurrentNetwork?.AliveState == true && ProcessUser.CurrentNetwork?.Network?.GetState() == true)
                {
                    return false;
                }
                else
                {
                    if (StopProcess(user.CurrentNetwork, false))
                        patchLocker.WaitOne();
                }
            }

            ProcessUser = user;

            initializeBackup();
            runScriptOnStart();
            var packet = new OutputPacketBuffer();
            packet.SetPacketId(Basic.PublisherClientPackets.ProjectPublishStart);

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

        public bool StopProcess(PublisherNetworkClient client, bool success, Dictionary<string, string> args = null)
        {
            if (ProcessUser == client.UserInfo)
            {
                if (client.CurrentFile != null)
                    EndFile(client);
                try { runScriptOnEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                bool isBackup = false;

                if (!success)
                    isBackup = success = recoveryBackup();

                if (success)
                {
                    if (isBackup)
                        ProcessFolder();
                    else
                        DumpFileList();

                    try { runScriptOnSuccessEnd(args ?? new Dictionary<string, string>()); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                    if (!isBackup)
                    {
                        Info.LatestUpdate = DateTime.UtcNow;
                        SaveProjectInfo();

                        broadcastUpdateTime();
                    }
                }
                else
                    try { runScriptOnFailedEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                ProcessUser = null;


                patchLocker.Set();

                while (WaitQueue.TryDequeue(out var newUser))
                {
                    if (newUser.CurrentNetwork?.AliveState == true && newUser.CurrentNetwork?.Network?.GetState() == true)
                    {
                        StartProcess(newUser);
                        break;
                    }
                }
                return true;
            }

            return false;
        }

        internal void StartFile(IProcessFileContainer client, string relativePath)
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
            else if (client.CurrentFile.FileInfo.Exists)
                addBackupFile(client.CurrentFile);


            runScriptOnFileStart(client.CurrentFile.Path);

            client.CurrentFile.StartFile();
        }

        internal void EndFile(IProcessFileContainer client)
        {
            if (client.CurrentFile == null)
                return;

            client.CurrentFile.EndFile();

            runScriptOnFileEnd(client.CurrentFile.Path);

            if (!FileInfoList.Contains(client.CurrentFile))
                FileInfoList.Add(client.CurrentFile);
            client.CurrentFile = null;
        }

        #endregion

        #region Patch

        private AutoResetEvent patchLocker = new AutoResetEvent(true);

        #region Server

        private ConcurrentBag<PublisherNetworkClient> patchClients = new ConcurrentBag<PublisherNetworkClient>();

        private PublisherNetworkClient currentDownloader = null;

        private void broadcastUpdateTime(PublisherNetworkClient client = null)
        {
            var packet = new OutputPacketBuffer();

            packet.SetPacketId(PatchClientPackets.ChangeLatestUpdateHandle);

            packet.WriteString16(Info.Id);
            packet.WriteDateTime(Info.LatestUpdate.Value);

            if (client == null)
                foreach (var item in patchClients)
                {
                    try { item.Send(packet); } catch { }
                }
            else
                client.Send(packet);
        }

        public SignStateEnum SignPatchClient(PublisherNetworkClient client, string userId, byte[] key, DateTime latestUpdate)
        {
            if (client.IsPatchClient == false)
            {
                client.IsPatchClient = true;
                client.PatchProjectMap = new Dictionary<string, ProjectInfo>();
            }

            var user = users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                Network.Packets.PathServer.SignInPacket.Send(client, SignStateEnum.UserNotFound);
                return SignStateEnum.UserNotFound;
            }

            if (user.Cipher == null)
            {
                user.Cipher = new RSACipher();

                user.Cipher.LoadXml(user.PSAPrivateKey);
            }

            byte[] data = user.Cipher.Decode(key, 0, key.Length);

            if (Encoding.ASCII.GetString(data) != userId)
            {
                Network.Packets.PathServer.SignInPacket.Send(client, SignStateEnum.UserNotFound);
                return SignStateEnum.UserNotFound;
            }

            patchClients.Add(client);

            client.PatchProjectMap.Add(Info.Id, this);

            client.RunAliveChecker();

            Network.Packets.PathServer.SignInPacket.Send(client, SignStateEnum.Ok);

            if (Info.LatestUpdate.HasValue && Info.LatestUpdate.Value > latestUpdate)
                broadcastUpdateTime(client);

            return SignStateEnum.Ok;
        }

        public void SignOutPatchClient(PublisherNetworkClient client)
        {
            patchClients = new ConcurrentBag<PublisherNetworkClient>(patchClients.Where(x => x != client));

            EndDownload(client, true);
        }

        public void StartDownload(PublisherNetworkClient client)
        {
            patchLocker.WaitOne();

            currentDownloader = client;
            client.PatchDownloadProject = this;

            Network.Packets.PathServer.StartDownloadPacket.Send(client, true, Info.IgnoreFilePaths);
        }

        public void NextDownloadFile(PublisherNetworkClient client, string relativePath)
        {
            if (client != currentDownloader)
                return;

            if (currentDownloader.CurrentFile != null)
                currentDownloader.CurrentFile.CloseRead();

            client.CurrentFile = FileInfoList.FirstOrDefault(x => x.RelativePath == relativePath);

            if (client.CurrentFile != null && !client.CurrentFile.FileInfo.Exists)
            {
                FileInfoList.Remove(client.CurrentFile);
                client.CurrentFile = null;
            }

            if (client.CurrentFile != null)
                client.CurrentFile.OpenRead();
        }

        internal void EndDownload<T>(T client, bool success = false)
            where T : INetworkClient, IProcessFileContainer
        {
            if (client != currentDownloader)
                return;

            if (client.CurrentFile != null)
            {
                client.CurrentFile.CloseRead();
                client.CurrentFile = null;
            }
            currentDownloader = null;
            if (client is PublisherNetworkClient c)
                c.PatchDownloadProject = null;

            patchLocker.Set();

            if (success)
            {
                var packet = new OutputPacketBuffer();

                packet.SetPacketId(PatchClientPackets.FinishDownloadResult);

                byte[] buf = null;

                packet.WriteCollection(Directory.GetFiles(ScriptsDirPath, "*.cs"), (p, d) =>
                {
                    buf = File.ReadAllBytes(d);
                    p.WritePath(Path.GetRelativePath(ProjectDirPath, d));
                    p.WriteInt32(buf.Length);
                    p.Write(buf);
                });

                client.Network?.Send(packet);
            }
        }

        #endregion

        private string PatchSignFilePath => Info.PatchInfo == null ? Guid.NewGuid().ToString() : Path.Combine(UsersPublicksDirPath, Info.PatchInfo.SignName + ".pubuk");

        public byte[] GetPatchSignData()
        {
            if (File.Exists(PatchSignFilePath))
                return File.ReadAllBytes(PatchSignFilePath);

            return null;
        }

        internal PatchClientNetwork PatchClient { get; private set; }

        internal ClientOptions<NetworkPatchClient> PatchClientOptions => PatchClient?.Options;

        private async void LoadPatch()
        {
            await LoadPatchAsync();
        }

        private async Task<bool> LoadPatchAsync()
        {
            if (Info.PatchInfo == null || !File.Exists(PatchSignFilePath))
                return false;

            PatchClient = await StaticInstances.PatchManager.LoadProjectPatchClient(this);

            if (PatchClient.GetState())
                await PatchClient.SignProject(this);

            return PatchClient != null;
        }

        public async void ClearPatchClient()
        {
            PatchClient = null;

            await Task.Delay(120_000);

            await LoadPatchAsync();
        }

        internal async Task Download(DateTime latestChangeTime)
        {
            patchLocker.WaitOne();

            if (!await PatchClient.InitializeDownload(this))
            {
                patchLocker.Set();
                DelayDownload(latestChangeTime);
                return;
            }

            initializeBackup();

            IEnumerable<BasicFileInfo> fileList = await PatchClient.GetFileList(this);

            if (fileList == default)
            {
                patchLocker.Set();
                DelayDownload(latestChangeTime);
                return;
            }

            if (Info.LatestUpdate.HasValue != false)
                fileList = fileList.Where(x => x.LastChanged > Info.LatestUpdate.Value);

            fileList = fileList.Where(x => !Info.IgnoreFilePaths.Any(ig => Regex.IsMatch(x.RelativePath, ig))).Reverse().ToList();
            runScriptOnStart();

            bool EOF = false;

            byte q = byte.MinValue;

            foreach (var file in fileList)
            {
                PatchClient.NextDownloadFile(file);
                StartFile(PatchClient.Options.ClientData, file.RelativePath);

                do
                {
                    await Task.Delay(50);
                    DownloadPacketData downloadProc = await PatchClient.Download();

                    if (downloadProc == default)
                    {
                        EndFile(PatchClient.Options.ClientData);
                        EndPatchReceive(false);
                        DelayDownload(latestChangeTime);
                        return;
                    }

                    EOF = downloadProc.EOF;

                    PatchClient.Options.ClientData.CurrentFile.IO.Write(downloadProc.Buff, 0, downloadProc.Buff.Length);
                    if (q % 10 == 0) PatchClient.Options.ClientData.CurrentFile.IO.Flush(true);

                    downloadProc.Dispose();
                    GC.Collect(GC.GetGeneration(downloadProc));
                    downloadProc = null;


                    if (q++ == byte.MaxValue / 10)
                    {
                        //GC.Collect(GC.GetGeneration(downloadProc));
                        GC.GetTotalMemory(true);
                        GC.WaitForFullGCComplete();
                        q = byte.MinValue;
                    }
                }
                while (EOF == false);
                //PatchClient.Options.ClientData.CurrentFile.CloseRead();
                EndFile(PatchClient.Options.ClientData);
                await Task.Delay(125);

            }

            var result = await PatchClient.FinishDownload(this);

            if (result == default)
            {
                EndPatchReceive(false);
                DelayDownload(latestChangeTime);
                return;
            }

            foreach (var item in result)
            {
                File.WriteAllBytes(Path.Combine(ProjectDirPath, item.fileName), item.data);
            }

            Info.LatestUpdate = latestChangeTime;

            getScript(true);

            EndPatchReceive(true);
        }

        private void EndPatchReceive(bool success)
        {
            try { runScriptOnEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

            bool isBackup = false;

            if (!success)
                isBackup = success = recoveryBackup();

            if (success)
            {
                if (isBackup)
                    ProcessFolder();
                else
                    DumpFileList();

                try { runScriptOnSuccessEnd(new Dictionary<string, string>()); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                if (!isBackup)
                {
                    Info.LatestUpdate = DateTime.UtcNow;
                    SaveProjectInfo();

                    broadcastUpdateTime();
                }
            }
            else
                try { runScriptOnFailedEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }
                patchLocker.Set();
        }

        private async void DelayDownload(DateTime latestChangeTime)
        {
            await Task.Delay(240_000);

            await Download(latestChangeTime);
        }

        #endregion

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
                if (Info.IgnoreFilePaths.Any(x => System.Text.RegularExpressions.Regex.IsMatch(filePath, x)))
                    continue;
                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);
            }
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
                user.PSAPrivateKey
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

        public void DumpFileList()
        {
            File.WriteAllText(CacheFilePath, System.Text.Json.JsonSerializer.Serialize(FileInfoList.Select(x => new { x.RelativePath, x.Hash })));
        }

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

        public ProjectInfo(string projectPath)
        {
            ProjectDirPath = projectPath;

            CreateDefault();
            CreateWatchers();
            LoadUsers();

            WaitQueue = new ConcurrentQueue<UserInfo>();

            LoadProjectInfo();

            LoadPatch();
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
    }
}
