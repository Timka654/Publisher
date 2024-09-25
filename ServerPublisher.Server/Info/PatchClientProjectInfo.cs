using ServerPublisher.Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ServerPublisher.Shared.Info;
using System.Threading;

namespace ServerPublisher.Server.Info
{
    public partial class ServerProjectInfo
    {
        private string PatchSignFilePath => Info.PatchInfo == null ? Guid.NewGuid().ToString() : Path.Combine(UsersPublicksDirPath, Info.PatchInfo.SignName + ".pubuk");

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
                TempPath = initializeTempPath()
            });
        }

        internal async Task Download(ProjectDownloadContext context)
        {
            patchLocker.WaitOne();

            if (currentDownloadTime > context.UpdateTime)
            {
                context.Dispose();
                return;
            }

            currentDownloadTime = context.UpdateTime;


            if (!await PatchClient.StartDownload(this))
            {
                FailedDownload(context);
                return;
            }

            context.FileList = await PatchClient.GetFileList(this);

            if (context.FileList == default)
            {
                FailedDownload(context);
                return;
            }

            context.FileList = context.FileList
                .Where(x => !Info.IgnoreFilePaths.Any(ig => Regex.IsMatch(x.RelativePath, ig)))
                .Where(x =>
                {
                    x.FileInfo = new FileInfo(Path.Combine(ProjectDirPath, x.RelativePath));

                    if (x.FileInfo.Exists == false)
                        return true;
                    string remote = x.Hash;
                    x.CalculateHash();
                    return x.Hash != remote;
                })
                .Reverse()
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
                File.WriteAllBytes(Path.Combine(ProjectDirPath, item.RelativePath), item.Data);
            }

            Info.LatestUpdate = context.UpdateTime;

            getScript(true);

            EndPatchReceive(context);

            patchLocker.Set();
        }

        private async Task<bool> downloadFile(ProjectDownloadContext context, DownloadFileInfo file)
        {
            bool EOF = false;

            byte q = byte.MinValue;

            var startResponse = await PatchClient.StartFileAsync(Info.Id, file.RelativePath);

            if (startResponse?.Result != true)
                return false;

            var fileInfo = new FileInfo(Path.Combine(context.TempPath, file.RelativePath));

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

            try { runScriptOnStart(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

            success = processTemp(context);

            if (!success)
                recoveryBackup();

            try { runScriptOnEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

            if (success)
            {
                DumpFileList();

                try { runScriptOnSuccessEnd(new Dictionary<string, string>()); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                Info.LatestUpdate = context.UpdateTime;
                SaveProjectInfo();

                broadcastUpdateTime();
            }
            else
                try { runScriptOnFailedEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

            patchLocker.Set();
        }
    }
}
