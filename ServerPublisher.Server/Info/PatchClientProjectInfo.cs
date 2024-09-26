using ServerPublisher.Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ServerPublisher.Shared.Info;
using System.Threading;
using ServerPublisher.Shared.Models.ResponseModel;
using ServerPublisher.Shared.Utils;

namespace ServerPublisher.Server.Info
{
    public partial class ServerProjectInfo
    {
        private string PatchSignFilePath => Info.PatchInfo == null ? Guid.NewGuid().ToString() : Path.Combine(UsersPublicksDirPath, Info.PatchInfo.SignName + ".pubuk").GetNormalizedPath();

        public byte[] GetPatchSignData()
        {
            if (File.Exists(PatchSignFilePath))
                return File.ReadAllBytes(PatchSignFilePath);

            return null;
        }

        internal PatchClientNetwork PatchClient { get; private set; }

        private async void LoadPatch()
        {
            await LoadPatchAsync();
        }

        private async Task<bool> LoadPatchAsync()
        {
            if (Info.PatchInfo == null || !File.Exists(PatchSignFilePath))
                return false;

            PatchClient = await PublisherServer.ProjectProxyManager.ConnectProxyClient(this);

            return PatchClient != null;
        }

        public async void ClearPatchClient()
        {
            PatchClient = null;

            await Task.Delay(120_000);

            await LoadPatchAsync();
        }

        private Func<ProjectProxyStartDownloadMessageModel, Task>? downloadUnlockAction = null;

        public async Task UnlockDownload(ProjectProxyStartDownloadMessageModel data)
        {
            var action = downloadUnlockAction;

            if (action == null)
                return;

            await action(data);
        }


        private DateTime currentDownloadTime;

        private void FailedDownload(ProjectDownloadContext context)
        {
            patchLocker.Set();

            context.Reload();
        }

        internal async Task Download(DateTime latestChangeTime)
        {
            await Download(new ProjectDownloadContext()
            {
                UpdateTime = latestChangeTime,
                ProjectInfo = this,
                TempPath = initializeTempPath(),
            });
        }

        internal async Task Download(ProjectDownloadContext context)
        {
            initializeDownloadLogger(context);

            patchLocker.WaitOne();

            if (currentDownloadTime > context.UpdateTime)
            {
                context.Dispose();
                return;
            }

            currentDownloadTime = context.UpdateTime;

            var waitLockerSource = new CancellationTokenSource();

            DownloadFileInfo[] fileList = null;

            downloadUnlockAction = (data) =>
            {
                downloadUnlockAction = null;

                fileList = data.FileList;

                waitLockerSource.Cancel();

                return Task.CompletedTask;
            };

            if (!await PatchClient.StartDownload(this))
            {
                FailedDownload(context);
                return;
            }

            try { await Task.Delay(-1, waitLockerSource.Token); } catch { }

            context.FileList = fileList
                .Where(x => !IgnorePathsPatters.Any(ig => Regex.IsMatch(x.RelativePath, ig)))
                .Where(x =>
                {
                    var ex = FileInfoList.FirstOrDefault(b => b.RelativePath == x.RelativePath);

                    return ex == null || x.Hash != ex.Hash;
                })
                .Select(x => new ProjectFileInfo(ProjectDirPath, new FileInfo(Path.Combine(ProjectDirPath, x.RelativePath).GetNormalizedPath()), this))
                .ToList();

            int offset = 0;
            do
            {
                var downloadFiles = context.FileList.Skip(offset).Take(10).ToList();

                var results = await Task.WhenAll(downloadFiles.Select(x => downloadFile(context, x)));

                if (results.Any(x => !x))
                {
                    FailedDownload(context);
                    return;
                }

                offset += downloadFiles.Count;
            } while (offset < context.FileList.Count());

            var result = await PatchClient.FinishDownload(this);

            if (result?.Success != true)
            {
                FailedDownload(context);
                return;
            }

            foreach (var item in result.FileList)
            {
                File.WriteAllBytes(Path.Combine(ProjectDirPath, item.RelativePath).GetNormalizedPath(), item.Data);
            }

            Info.LatestUpdate = context.UpdateTime;

            getScript(true);

            EndPatchReceive(context);

            patchLocker.Set();
        }

        private async Task<bool> downloadFile(ProjectDownloadContext context, ProjectFileInfo file)
        {
            bool EOF = false;

            byte q = byte.MinValue;

            var startResponse = await PatchClient.StartFileAsync(Info.Id, file.RelativePath);

            if (startResponse?.Result != true)
                return false;

            var fileInfo = new FileInfo(Path.Combine(context.TempPath, file.RelativePath).GetNormalizedPath());

            using var fs = fileInfo.Create();

            do
            {
                await Task.Delay(20);
                var downloadProc = await PatchClient.DownloadAsync(startResponse.FileId);

                if (downloadProc == default)
                    return false;

                EOF = downloadProc.EOF;

                fs.Write(downloadProc.Bytes);

                if (++q % 10 == 0)
                {
                    fs.Flush();
                    GC.GetTotalMemory(true);
                    GC.WaitForFullGCComplete();
                    q = byte.MinValue;
                }
            }
            while (EOF == false);

            var stopResponse = await PatchClient.StopFileAsync(startResponse.FileId);

            if (stopResponse?.Result != true)
                return false;

            await Task.Delay(125);

            return true;
        }

        private void EndPatchReceive(ProjectDownloadContext context)
        {
            bool success = false;

            finishPublishProcessOnStartScript(true, ref success);

            if (success)
            {
                success = processTemp(context);

                if (!success)
                    recoveryBackup();
            }

            FinishPublishProcessOnEndScript(true, ref success, new Dictionary<string, string>());

            if (success)
            {
                DumpFileList();

                Info.LatestUpdate = context.UpdateTime;

                SaveProjectInfo();

                broadcastUpdateTime();
            }

            patchLocker.Set();
        }
    }
}
