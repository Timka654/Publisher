using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServerPublisher.Server.Managers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using ServerPublisher.Server.Network.PublisherClient;
using System.IO.Compression;
using System.Runtime.InteropServices;
using NSL.SocketCore.Utils.Buffer;
using Newtonsoft.Json;
using NSL.Logger;
using ServerPublisher.Server.Scripts;
using NSL.Utils;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using ServerPublisher.Server.Utils;
using System.ComponentModel;
using System.Threading;

namespace ServerPublisher.Server.Info
{
    public partial class ServerProjectInfo : IDisposable, IScriptableServerProjectInfo
    {
        #region Path

        public string ProjectDirPath { get; private set; }

        public string PublisherDirPath => Path.Combine(ProjectDirPath, "Publisher");

        public string ProjectBackupPath => Path.Combine(PublisherDirPath, "Backup");

        public string ProjectFilePath => Path.Combine(PublisherDirPath, "project.json");

        public string CacheFilePath => Path.Combine(PublisherDirPath, "cache.json");

        public string UsersDirPath => Path.Combine(PublisherDirPath, "users");

        public string UsersPublicsDirPath => Path.Combine(UsersDirPath, "publ");

        public string TempDirPath => Path.Combine(PublisherDirPath, "temp");

        public string LogsDirPath => Path.Combine(PublisherDirPath, "logs");


        #region Scripts

        public static string GlobalScriptsDirPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.GlobalScriptsFolderPath);
        public static string[] ScriptsDefaultUsings => PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.ScriptsDefaultUsings;

        public string ScriptsDirPath => Path.Combine(PublisherDirPath, "scripts");

        public string OnStartScriptPath => Path.Combine(ScriptsDirPath, "OnStart.cs");

        public string OnEndScriptPath => Path.Combine(ScriptsDirPath, "OnEnd.cs");

        public string OnFileStartScriptPath => Path.Combine(ScriptsDirPath, "OnFileStart.cs");

        public string OnFileEndScriptPath => Path.Combine(ScriptsDirPath, "OnFileEnd.cs");

        #endregion

        #endregion

        #region Scripts

        public bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        DateTime? scriptLatestBuilded;

        DateTime scriptLatestChanged = DateTime.UtcNow;

        private delegate void startMethodDelegate(ScriptInvokingContext context, bool success, bool postProcessingSuccess);
        private delegate void endMethodDelegate(ScriptInvokingContext context, bool success, bool postProcessingSuccess, Dictionary<string, string> args);

        private delegate void fileMethodDelegate(ScriptInvokingContext context, IScriptableFileInfo file);

        private startMethodDelegate? OnStartMethod;

        private endMethodDelegate? OnEndMethod;

        private fileMethodDelegate? OnFileStartMethod;

        private fileMethodDelegate? OnFileEndMethod;

        private bool loadScripts(bool ignoreCache = false)
        {
            if (scriptLatestBuilded.HasValue && scriptLatestBuilded >= scriptLatestChanged && ignoreCache == false)
                return true;

            string[] usings = ScriptsDefaultUsings;

            var scriptUsings = string.Join("\n", usings.Select(x => $"using {x};")) + "\n";

            var privateCoreLibPath = typeof(Object).GetTypeInfo().Assembly.Location;

            var coreDir = Directory.GetParent(privateCoreLibPath);
            var compilation = CSharpCompilation.Create("CustomAssembly",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(privateCoreLibPath))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(coreDir.FullName, "mscorlib.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(coreDir.FullName, "System.Runtime.dll")))
            .AddReferences(MetadataReference.CreateFromFile(Path.Combine(coreDir.FullName, "System.Collections.dll")))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Diagnostics.Process).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClient).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Component).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IScriptableServerProjectInfo).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(ServerProjectInfo).Assembly.Location));

            try
            {
                foreach (var item in PublisherServer.Configuration.Publisher.ProjectConfiguration.Server.ScriptsReferences.Concat(this.Info.ScriptsReferences))
                {
                    if (!string.IsNullOrWhiteSpace(item.DllAbsolutePath))
                    {
                        var path = item.DllAbsolutePath;

                        if(File.Exists(path))
                            throw new Exception($"Cannot absolute path \"{path}\"");

                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(path));
                    }
                    if (!string.IsNullOrWhiteSpace(item.DllProjectPath))
                    {
                        var path = Path.Combine(ProjectDirPath, item.DllProjectPath.TrimStart('/', '\\'));

                        if(File.Exists(path))
                            throw new Exception($"Cannot relative path \"{path}({item.DllProjectPath})\"");

                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(path));
                    }
                    if (!string.IsNullOrWhiteSpace(item.DllRelativePath))
                    {
                        var path = Path.Combine(coreDir.FullName, item.DllRelativePath.TrimStart('/', '\\'));

                        if(File.Exists(path))
                            throw new Exception($"Cannot relative path \"{path}({item.DllRelativePath})\"");

                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(path));
                    }
                    else if (!string.IsNullOrWhiteSpace(item.ImportType))
                    {
                        var type = Type.GetType(item.ImportType, true);

                        if (type == null)
                            throw new Exception($"Cannot found type \"{item.ImportType}\"");

                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(type.Assembly.Location));
                    }
                }
            }
            catch (Exception ex)
            {
                var err = $"Project ({Info.Id}) references error - {ex}";

                PublisherServer.ServerLogger.AppendError(err);

                BroadcastMessage(err);
                return false;
            }


            if (Directory.Exists(ScriptsDirPath))
                compilation = compilation.AddSyntaxTrees(Directory.GetFiles(ScriptsDirPath, "*.cs").Select(x => CSharpSyntaxTree.ParseText(scriptUsings + File.ReadAllText(x), path: x)));
            if (Directory.Exists(GlobalScriptsDirPath))
                compilation = compilation.AddSyntaxTrees(Directory.GetFiles(GlobalScriptsDirPath, "*.cs").Select(x => CSharpSyntaxTree.ParseText(scriptUsings + File.ReadAllText(x), path: x)));


            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    PublisherServer.ServerLogger.AppendError($"Project ({Info.Id}) script compilation errors");

                    foreach (var item in result.Diagnostics)
                    {
                        var loc = item.Location;

                        var srcText = item.Location.SourceTree.GetText().ToString();

                        var line = srcText[..loc.SourceSpan.Start].Count(x => x == '\n') - usings.Length + 1;
                        var start = srcText[..loc.SourceSpan.Start].LastIndexOf('\n');

                        start = loc.SourceSpan.Start - start;

                        var err = $"-{loc.SourceTree.FilePath}({line},{start}) {item.GetMessage()}";

                        PublisherServer.ServerLogger.AppendError(err);
                        BroadcastMessage(err);
                    }

                    return false;
                }

                var asm = Assembly.Load(ms.ToArray());

                OnStartMethod = asm.GetScriptMethod("OnStart")?.CreateDelegate<startMethodDelegate>();

                OnEndMethod = asm.GetScriptMethod("OnEnd")?.CreateDelegate<endMethodDelegate>();

                OnFileStartMethod = asm.GetScriptMethod("OnFileStart")?.CreateDelegate<fileMethodDelegate>();

                OnFileEndMethod = asm.GetScriptMethod("OnFileEnd")?.CreateDelegate<fileMethodDelegate>();
            }

            scriptLatestBuilded = DateTime.UtcNow;

            return true;
        }

        internal void CheckScripts()
        {
            loadScripts();
        }

        public void BroadcastMessage(string log)
        {
            var packet = OutputPacketBuffer.Create(PublisherPacketEnum.ServerLog);

            packet.WriteString(log);

            foreach (var item in ConnectedPublishers.Values.ToArray())
            {
                item.Send(packet);
            }
        }

        #endregion

        #region Watchers

        private void CreateWatchers(ProjectsManager projectsManager)
        {
            PubUsersWatch = new FSWatcher(() => new FileSystemWatcher(UsersPublicsDirPath, "*.pubuk"))
            {
                OnCreated = PubUsersWatch_Changed,
                OnChanged = PubUsersWatch_Changed,
                OnDeleted = PubUsersWatch_Deleted
            };

            UsersWatch = new FSWatcher(() => new FileSystemWatcher(UsersDirPath, "*.priuk"))
            {
                OnChanged = UsersWatch_Changed,
                OnDeleted = UsersWatch_Deleted

            };
            SettingsWatch = new FSWatcher(() => new FileSystemWatcher(PublisherDirPath, new FileInfo(ProjectFilePath).Name))
            {
                OnChanged = SettingsWatch_Changed,
                OnDeleted = SettingsWatch_Deleted
            };
            ScriptsWatch = new FSWatcher(() => new FileSystemWatcher(ScriptsDirPath, "*.cs"))
            {
                OnAnyChanges = ScriptsWatch_Changed
            };

            GlobalScriptsWatch = new FSWatcher(() => new FileSystemWatcher(GlobalScriptsDirPath, "*.cs"))
            {
                OnAnyChanges = ScriptsWatch_Changed
            };

            projectsManager.GlobalBothUserProxyStorage.OnCreated += GlobalBothUserStorage_OnCreated;
            projectsManager.GlobalProxyUserStorage.OnCreated += GlobalBothUserStorage_OnCreated;
        }

        private void GlobalBothUserStorage_OnCreated(UserInfo obj)
        {
            if (obj.FileName == Path.GetFileName(PatchSignFilePath))
            {
                LoadPatch();
            }
        }

        private void ScriptsWatch_Changed(FileSystemEventArgs e)
        {
            scriptLatestChanged = DateTime.UtcNow;
        }

        private void SettingsWatch_Deleted(FileSystemEventArgs e)
        {
            projectsManager.RemoveProject(this);
        }

        private void SettingsWatch_Changed(FileSystemEventArgs e)
        {
            var oldInfo = Info;

            LoadProjectInfo();

            PublisherServer.ServerLogger.AppendInfo($"{ProjectFilePath} changed \r\nold {JsonConvert.SerializeObject(oldInfo)}\r\nnew {JsonConvert.SerializeObject(Info)}");


            if (oldInfo.PatchInfo == null && Info.PatchInfo != null)
            {
                if (PatchClient == null)
                    LoadPatch();
            }
            else if (oldInfo.PatchInfo != null && Info.PatchInfo == null)
            {
                if (PatchClient != null)
                {
                    PatchClient?.SignOutProject(this);
                    PatchClient = null;
                }
            }
            else if (oldInfo.PatchInfo != null && Info.PatchInfo != null)
            {
                if (oldInfo.PatchInfo.IpAddress != Info.PatchInfo.IpAddress ||
                oldInfo.PatchInfo.Port != Info.PatchInfo.Port ||
                oldInfo.PatchInfo.SignName != Info.PatchInfo.SignName ||
                oldInfo.PatchInfo.InputCipherKey != Info.PatchInfo.InputCipherKey ||
                oldInfo.PatchInfo.OutputCipherKey != Info.PatchInfo.OutputCipherKey)
                {

                    PatchClient?.SignOutProject(this);

                    LoadPatch();
                }
            }
        }

        private void PubUsersWatch_Deleted(FileSystemEventArgs e)
        {
            if (e.FullPath.GetNormalizedPath() == PatchSignFilePath)
            {
                PatchClient?.SignOutProject(this);
            }
        }

        private void PubUsersWatch_Changed(FileSystemEventArgs e)
        {
            if (e.FullPath.GetNormalizedPath() == PatchSignFilePath)
            {
                PatchClient?.SignOutProject(this);
                LoadPatch();
            }
        }

        private void UsersWatch_Deleted(FileSystemEventArgs e)
        {
            users.RemoveAll(users.Where(x =>
            {
                if (x.FileName != e.FullPath) return false;
                x.Dispose(); return true;
            }).ToArray().Contains);
        }

        private void UsersWatch_Changed(FileSystemEventArgs e)
        {
            var user = new UserInfo(e.FullPath);
            AddOrUpdateUser(user);
        }

        #endregion

        #region Logger

        private void initializePublishLogger(ProjectPublishContext context)
        {
            var uid = context.Network.UserInfo.Id ?? "Unknown";

            context.Logger = new FileLogger(LogsDirPath, $"{DateTime.Now:yyyy-MM-dd_HH.mm.ss} - upload - {uid}");
        }

        private void initializeDownloadLogger(ProjectDownloadContext context)
        {
            context.Logger = new FileLogger(LogsDirPath, $"{DateTime.Now:yyyy-MM-dd_HH.mm.ss} - download");
        }

        #endregion

        #region Temp

        private string initializeTempPath()
        {
            var di = new DirectoryInfo(TempDirPath);

            if (!di.Exists)
                di.Create();

            return di.CreateSubdirectory($"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid()}").GetNormalizedDirectoryPath();
        }

        private async Task<bool> processCompressedTemp(ProjectPublishContext context)
        {
            var archiveFile = context.FileMap.Values.First();

            context.FileMap.Clear();

            context.Log("-> Start Archive processing");

            var archivePath = Path.Combine(context.TempPath, archiveFile.file.RelativePath).GetNormalizedPath();

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var archiveEntry in archive.Entries)
                {
                    var id = startPublishFile(
                         context, new PublishProjectFileStartRequestModel()
                         {
                             RelativePath = Path.Combine(context.OutputRelativePath ?? string.Empty, CorrectCompressedPath(context, archiveEntry.FullName)).GetNormalizedPath(),
                             CreateTime = archiveEntry.LastWriteTime.DateTime,
                             UpdateTime = archiveEntry.LastWriteTime.DateTime,
                         });

                    if (!id.HasValue)
                    {
                        return false;
                    }

                    var file = context.FileMap[id.Value];

                    using (var ex = archiveEntry.Open())
                    {
                        await file.file.WriteAsync(ex);
                    }

                    endPublishFile(context, file.file);
                }
            }

            await Task.Delay(1_000);

            File.Delete(archivePath);

            context.Log("-> Finish Archive processing");

            return await processTemp(context, false);
        }

        private string CorrectCompressedPath(ProjectPublishContext context, string path)
        {
            var client = context.Network;

            if (client.Platform == CurrentOS)
                return path;

            if (client.Platform == OSTypeEnum.Windows)
                return path.Replace('\\', '/');

            return path.Replace('/', '\\');
        }

        private async Task<bool> processTemp(IProcessingFilesContext context, bool checkHash)
        {
            try
            {
                bool canBackup = initializeBackup();

                if (Info.FullReplace)
                {
                    foreach (var item in FileInfoList)
                    {
                        if (canBackup)
                            createFileBackup(item);

                        item.RemoveFile();
                    }
                }

                var execContext = new ScriptInvokingContext(this, context);


                var updateFiles = context.GetFiles();

                var results = await Task.WhenAll(updateFiles.Select(item => Task.Run(() =>
                {


                    context.Log($"-> Start file processing {item.RelativePath}");

                    if (!Info.FullReplace && canBackup)
                        createFileBackup(item);

                    OnFileStartMethod?.Invoke(execContext, item);

                    if (item.TempRelease(context, checkHash) == false)
                    {
                        context.Log($"Error!! cannot move file from temp {item.RelativePath}!!");

                        return false;
                    }

                    OnFileEndMethod?.Invoke(execContext, item);

                    context.Log($"-> Finish file processing {item.RelativePath}");

                    return true;
                })));

                if (results.Any())
                    return !results.Any(x => x == false);

            }
            catch (Exception ex)
            {
                context.Log(ex.ToString(), true);
                return false;
            }

            return true;
        }

        #endregion

        #region Backup

        private string? currentBackupDirPath = null;

        private bool initializeBackup()
        {
            if (Info.Backup == false)
                return false;

            currentBackupDirPath = Path.Combine(ProjectBackupPath, DateTime.UtcNow.ToString("yyyy-MM-ddTHH_mm_ss")).GetNormalizedPath();
            return true;
        }

        private void createFileBackup(ProjectFileInfo file)
        {
            if (Info.Backup == false)
                return;

            if (file.FileInfo.Exists == false)
                return;

            if (currentBackupDirPath == null)
                throw new Exception($"Cannot create backup - {nameof(initializeBackup)} not be executed before process");

            var fi = new FileInfo(Path.Combine(currentBackupDirPath, file.RelativePath).GetNormalizedPath());

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            file.FileInfo.CopyTo(fi.GetNormalizedFilePath());
        }

        private bool recoveryBackup()
        {
            if (Info.Backup == false)
                return false;

            if (currentBackupDirPath == null)
                return true;

            var dir = new DirectoryInfo(currentBackupDirPath);

            if (dir.Exists == false)
                return true;

            foreach (var item in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                item.CopyTo(Path.Combine(ProjectDirPath, item.GetNormalizedRelativePath(dir).GetNormalizedPath()).GetNormalizedPath(), true);
            }

            dir.Delete(true);

            currentBackupDirPath = null;

            return true;
        }

        #endregion

        #region Publish

        private ProjectPublishContext? CurrentPublishContext { get; set; }

        private AutoResetEvent signProcessLocker = new AutoResetEvent(true);

        public async Task<bool> StartPublishProcess(PublisherNetworkClient client)
        {
            var context = client.PublishContext;

            if (context.ProjectInfo != this)
                return false;

            signProcessLocker.WaitOne();

            ConnectedPublishers.TryAdd(context.Id, client);

            if (!patchLocker.WaitOne(0))
            {
                await Task.Delay(1_000);

                var cpc = CurrentPublishContext;

                if (cpc?.Network?.GetState(true) == true)
                {
                    if (!WaitPublishQueue.Contains(client))
                        WaitPublishQueue.Enqueue(client);

                    return false;
                }
                else if (cpc != null && cpc.FinishProcessing != true)
                {
                    await FinishPublishProcess(cpc, false, null);
                }
            }

            var s = StartPublishProcess(context);

            signProcessLocker.Set();

            return s;
        }

        public bool StartPublishProcess(ProjectPublishContext context)
        {
            if (context.ProjectInfo != this)
            {
                NextPublisherOrUnlock();
                return false;
            }

            CurrentPublishContext = context;

            context.Actual = context.Network?.GetState(true) == true;

            if (context.Actual == false)
            {
                NextPublisherOrUnlock();
                return false;
            }

            context.TempPath = initializeTempPath();

            initializePublishLogger(context);

            if (context.Network?.GetState(true) != true)
            {
                context.Actual = false;
                NextPublisherOrUnlock();
                return false;
            }

            var packet = OutputPacketBuffer.Create(PublisherPacketEnum.PublishProjectStartMessage);

            new ProjectFileListResponseModel()
            {
                FileList = Info.FullReplace ? [] : FileInfoList.Cast<BasicFileInfo>().ToArray()
            }

            .WriteDefaultTo(packet);

            context.Network?.Send(packet);

            return true;
        }

        private async void NextPublisherOrUnlock()
        {
            while (WaitPublishQueue.TryDequeue(out var newUser))
            {
                if (newUser?.GetState(true) == true)
                {
                    await StartPublishProcess(newUser);
                    return;
                }
            }

            patchLocker.Set();
        }


        public async Task<bool> FinishPublishProcess(ProjectPublishContext context, bool success, Dictionary<string, string>? args = null)
        {
            if (!context.Actual || CurrentPublishContext != context)
            {
                context.Actual = false;
                return false;
            }

            if (!loadScripts())
            {
                context.Log($"Scripts have errors - lock update", true);

                return false;
            }
            context.FinishProcessing = true;

            bool successProcess = true;

            finishPublishProcessOnStartScript(context, success, ref successProcess);

            if (success && successProcess)
            {
                successProcess = await finishPublishProcessProduceFiles(context);
            }

            FinishPublishProcessOnEndScript(context, success, ref successProcess, args);

            if (success && successProcess)
            {
                ReleaseUpdateContext(context);

                Info.LatestUpdate = DateTime.UtcNow;

                SaveProjectInfo();

                broadcastUpdateTime();
            }
            else
            {
                FinishPublishProcessRecoveryBackup();

                FinishPublishProcessOnEndScript(context, success, ref successProcess, args);
            }

            context.Actual = false;
            CurrentPublishContext = null;

            NextPublisherOrUnlock();

            return successProcess;
        }

        private void finishPublishProcessOnStartScript(IProcessingFilesContext context, bool success, ref bool successProcess)
        {
            try { OnStartMethod?.Invoke(new ScriptInvokingContext(this, context), success, false); } catch (Exception ex) { context.Log(ex.ToString()); successProcess = false; }
        }

        private async Task<bool> finishPublishProcessProduceFiles(ProjectPublishContext context)
        {
            if (!context.FileMap.Any())
                return true;

            try
            {
                if (context.UploadMethod == UploadMethodEnum.SingleArchive)
                    return await processCompressedTemp(context);
                else
                    return await processTemp(context, true);
            }
            catch (Exception ex)
            {
                context.Log(ex.ToString());
            }

            return false;
        }

        private void FinishPublishProcessRecoveryBackup()
        {
            recoveryBackup();
        }

        private void FinishPublishProcessOnEndScript(IProcessingFilesContext context, bool success, ref bool postProcessingSuccess, Dictionary<string, string> args)
        {
            try { OnEndMethod?.Invoke(new ScriptInvokingContext(this, context), success, postProcessingSuccess, args); } catch (Exception ex) { context.Log(ex.ToString()); postProcessingSuccess = false; }
        }

        internal Guid? StartPublishFile(ProjectPublishContext context, PublishProjectFileStartRequestModel data)
        {
            if (!context.Actual || CurrentPublishContext != context)
            {
                context.Actual = false;
                return default;
            }

            return startPublishFile(context, data);
        }

        private Guid? startPublishFile(ProjectPublishContext context, PublishProjectFileStartRequestModel data)
        {
            var file = new ProjectFileInfo(ProjectDirPath, new FileInfo(Path.Combine(ProjectDirPath, data.RelativePath).GetNormalizedPath()), this);

            Guid id = default;

            while (!context.FileMap.TryAdd(id = Guid.NewGuid(), (new ProjectPublishContext.UploadFileInfo(data.Length), file))) ;

            file.StartFile(context, data.CreateTime, data.UpdateTime, data.Hash);

            return id;
        }

        internal async Task<bool> UploadPublishFile(ProjectPublishContext context, PublishProjectUploadFileBytesRequestModel request)
        {
            if (!context.Actual || CurrentPublishContext != context)
            {
                context.Actual = false;
                return false;
            }

            if (!context.FileMap.TryGetValue(request.FileId, out var file))
                return false;

            await file.file.WriteAsync(request.Bytes, request.Offset, () =>
            {
                file.upload.Offset += request.Bytes.Length;
                return Task.CompletedTask;
            });


            request.Bytes = null;

            if (file.upload.Offset == file.upload.len)
                EndPublishFile(context, file.file);


            return true;
        }

        internal void EndPublishFile(ProjectPublishContext context, ProjectFileInfo file)
        {
            if (!context.Actual || CurrentPublishContext != context)
            {
                context.Actual = false;
                return;
            }

            endPublishFile(context, file);
        }

        internal void endPublishFile(ProjectPublishContext context, ProjectFileInfo file)
        {
            file.EndFile(context);
        }

        #endregion

        #region Files

        private void ProcessFolder()
        {
            if (!File.Exists(CacheFilePath))
            {
                ProcessFolderData();
                dumpFileList();
            }
            else
            {
                string cacheJson = File.ReadAllText(CacheFilePath);

                if (!string.IsNullOrWhiteSpace(cacheJson))
                    FileInfoList = System.Text.Json.JsonSerializer.Deserialize<List<ProjectFileInfo>>(cacheJson);
                else
                    FileInfoList = new List<ProjectFileInfo>();

                PublisherServer.ServerLogger.AppendInfo($"Project {Info.Name}({Info.Id}) Loaded {FileInfoList.Count} files");

                var removed = new List<ProjectFileInfo>();

                foreach (var item in FileInfoList)
                {
                    item.FileInfo = new FileInfo(Path.Combine(ProjectDirPath, item.RelativePath).GetNormalizedPath());
                    item.Project = this;
                    if (!item.FileInfo.Exists)
                        removed.Add(item);
                }

                PublisherServer.ServerLogger.AppendInfo($"Project {Info.Name}({Info.Id}) Removed {FileInfoList.RemoveAll(x => removed.Contains(x))} invalid files");

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
                filePath = Path.GetRelativePath(ProjectDirPath, file.GetNormalizedFilePath()).GetNormalizedPath();

                if (IgnorePathsPatters.Any(x => Regex.IsMatch(filePath, x)))
                    continue;

                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);
            }
        }

        public void ReleaseUpdateContext(IProcessingFilesContext context)
        {
            var updatedPaths = context.GetFiles().Select(x => x.RelativePath).ToArray();

            var newFileList = FileInfoList.Where(x => !updatedPaths.Contains(x.RelativePath)).ToList();

            newFileList.AddRange(context.GetFiles());

            FileInfoList = newFileList;

            dumpFileList();
        }

        private void dumpFileList()
        {
            File.WriteAllText(CacheFilePath, System.Text.Json.JsonSerializer.Serialize(FileInfoList.Select(x => new
            {
                x.RelativePath,
                x.Hash
            })));
        }

        internal void ReIndexing()
        {
            PublisherServer.ServerLogger.AppendInfo($"Try reindexing project {Info.Name}({Info.Id})");

            ProcessFolder();

            string oldHash = default;
            bool exists = false;

            foreach (var item in FileInfoList)
            {
                oldHash = item.Hash;

                item.CalculateHash();

                if (oldHash != item.Hash)
                {
                    PublisherServer.ServerLogger.AppendInfo($"{item.RelativePath} invalid hash ({oldHash} vs {item.Hash})");
                    exists = true;
                }
            }

            var di = new DirectoryInfo(ProjectDirPath);

            string filePath = "";

            foreach (var file in RecurciveFiles(di))
            {
                filePath = Path.GetRelativePath(ProjectDirPath, file.GetNormalizedFilePath()).GetNormalizedPath();

                if (IgnorePathsPatters.Any(x => Regex.IsMatch(filePath, x)))
                    continue;

                if (FileInfoList.Any(x => x.RelativePath == filePath))
                    continue;

                var pfi = new ProjectFileInfo(ProjectDirPath, file, this);

                pfi.CalculateHash();

                FileInfoList.Add(pfi);

                PublisherServer.ServerLogger.AppendInfo($"{pfi.RelativePath} new file {pfi.Hash}");
            }

            if (exists)
            {
                dumpFileList();
                Info.LatestUpdate = DateTime.UtcNow;
                SaveProjectInfo();
                PublisherServer.ServerLogger.AppendInfo($"Success reindexing project");
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
                PublisherServer.ServerLogger.AppendError(ex.ToString());
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
                return false;
            }

            user.ProducePublicKey(UsersPublicsDirPath);
            user.ProducePrivateKey(UsersDirPath);

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
            ProcessFolder();
        }

        public ProjectInfoData Info { get; private set; }

        public List<ProjectFileInfo> FileInfoList { get; private set; }

        private ConcurrentQueue<PublisherNetworkClient> WaitPublishQueue { get; set; }

        public ConcurrentDictionary<Guid, PublisherNetworkClient> ConnectedPublishers { get; } = new ConcurrentDictionary<Guid, PublisherNetworkClient>();


        private readonly List<UserInfo> users = new List<UserInfo>();
        private readonly ProjectsManager projectsManager;
        public FSWatcher PubUsersWatch;

        public FSWatcher UsersWatch;

        public FSWatcher SettingsWatch;

        public FSWatcher ScriptsWatch;

        public FSWatcher GlobalScriptsWatch;

        private void CheckDefault()
        {
            if (!Directory.Exists(UsersDirPath))
                Directory.CreateDirectory(UsersDirPath);

            if (!Directory.Exists(UsersPublicsDirPath))
                Directory.CreateDirectory(UsersPublicsDirPath);

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
                    PublisherServer.ServerLogger.AppendError($"cannot read {item} user data, exception: {ex.ToString()}");
                }
            }
        }

        public string[] IgnorePathsPatters
            => Info.IgnoreFilePaths.Concat(PublisherServer.Configuration.Publisher.ProjectConfiguration.Base.IgnoreFilePaths).ToArray();

        public ServerProjectInfo(string projectPath, ProjectsManager projectsManager)
        {
            ProjectDirPath = projectPath;
            this.projectsManager = projectsManager;
            CheckDefault();
            CreateWatchers(projectsManager);
            LoadUsers();

            WaitPublishQueue = new();

            LoadProjectInfo();

            LoadPatch();

            loadScripts();
        }

        public ServerProjectInfo(ProjectInfoData pid, string directory)
        {
            ProjectDirPath = directory;

            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "project_template").GetNormalizedPath();

            if (Directory.Exists(templatePath))
                DirectoryCopy(templatePath, PublisherDirPath, true);

            Info = pid;

            Info.IgnoreFilePaths ??= PublisherServer.Configuration.Publisher.ProjectConfiguration.Default.IgnoreFilePaths.ToList();

            CheckDefault();

            SaveProjectInfo();
        }

        public ServerProjectInfo(string? id, string name, bool fullReplace, bool backup, string directory)
        {
            PublisherServer.ServerLogger.AppendInfo("project creating");

            Info = new ProjectInfoData()
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = name,
                FullReplace = fullReplace,
                Backup = backup,
                IgnoreFilePaths = PublisherServer.Configuration.Publisher.ProjectConfiguration.Default.IgnoreFilePaths.ToList()
            };

            ProjectDirPath = directory;

            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "project_template").GetNormalizedPath();

            if (Directory.Exists(templatePath))
                DirectoryCopy(templatePath, PublisherDirPath, true);

            CheckDefault();

            SaveProjectInfo();
        }

        public void SaveProjectInfo()
        {
            File.WriteAllText(ProjectFilePath, JsonConvert.SerializeObject(Info, JsonUtils.JsonSettings));
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
            PubUsersWatch.Dispose();
            UsersWatch.Dispose();
            SettingsWatch.Dispose();
            ScriptsWatch.Dispose();
            GlobalScriptsWatch.Dispose();

            if (PatchClient != null)
                PatchClient.SignOutProject(this);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
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
                string tempPath = Path.Combine(destDirName, file.Name).GetNormalizedPath();

                if (File.Exists(tempPath) && !overwrite)
                    continue;

                file.CopyTo(tempPath, overwrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name).GetNormalizedPath();
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }


        private static OSTypeEnum CurrentOS;

        static ServerProjectInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CurrentOS = OSTypeEnum.Windows;
            else
                CurrentOS = OSTypeEnum.Unix;
        }
    }

    public class ProjectDownloadContext : IProcessingFilesContext, IDisposable
    {
        public DateTime UpdateTime { get; set; }

        public string TempPath { get; set; }

        public ServerProjectInfo ProjectInfo { get; set; }

        public IEnumerable<ProjectFileInfo> FileList { get; set; }

        public FileLogger? Logger { get; set; }

        public void Log(string text, bool appLog = false)
        {
            if (appLog)
                PublisherServer.ServerLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, text);

            Logger?.AppendLog(text);

            ProjectInfo.BroadcastMessage(text);
        }

        public async void Reload()
        {
            await Task.Delay(TimeSpan.FromSeconds(20));

            await ProjectInfo.Download(this);

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProjectFileInfo> GetFiles()
            => FileList;

        public bool AnyFiles()
            => FileList.Any();
    }

    public class ProjectPublishContext : IProcessingFilesContext, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public bool Actual { get; set; }

        public string? TempPath { get; set; }

        public ServerProjectInfo ProjectInfo { get; set; }

        public PublisherNetworkClient Network { get; set; }

        public FileLogger? Logger { get; set; }

        public OSTypeEnum Platform { get; set; }

        public UploadMethodEnum UploadMethod { get; set; }

        public string? OutputRelativePath { get; set; }

        public bool FinishProcessing { get; set; }

        public ConcurrentDictionary<Guid, (UploadFileInfo upload, ProjectFileInfo file)> FileMap { get; } = new();

        public record UploadFileInfo(long len)
        {
            public long Offset { get; set; }
        }

        public void Log(string text, bool appLog = false)
        {
            if (appLog)
                PublisherServer.ServerLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, text);

            Logger?.AppendLog(text);

            ProjectInfo.BroadcastMessage(text);
        }

        public void Dispose()
        {
            ProjectInfo.ConnectedPublishers.TryRemove(Id, out _);

            if (!FinishProcessing)
            {
                foreach (var item in FileMap.Values)
                {
                    item.file.ReleaseIO();
                }

                FileMap.Clear();
                if (TempPath != default)
                    Directory.Delete(TempPath, true);
            }

            ProjectInfo?.FinishPublishProcess(this, false);

            Actual = false;

            Logger?.Dispose();
        }

        public IEnumerable<ProjectFileInfo> GetFiles()
            => FileMap.Select(x => x.Value.file).ToArray();

        public bool AnyFiles()
            => FileMap.Any();
    }

    public interface IProcessingFilesContext : IScriptableExecutorContext
    {
        string? TempPath { get; }

        IEnumerable<ProjectFileInfo> GetFiles();

        void Log(string content, bool appLog = false);
    }
}
