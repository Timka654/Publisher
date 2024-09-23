using ServerPublisher.Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NSL.SocketClient;
using ServerPublisher.Shared.Info;

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

        internal ClientOptions<NetworkPatchClient> PatchClientOptions => PatchClient?.Options;

        private async void LoadPatch()
        {
            await LoadPatchAsync();
        }

        private async Task<bool> LoadPatchAsync()
        {
            if (Info.PatchInfo == null || !File.Exists(PatchSignFilePath))
                return false;

            PatchClient = await PublisherServer.ProjectProxyManager.LoadProjectPatchClient(this);

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

            initializeLogger();
            initializeTemp();

            IEnumerable<DownloadFileInfo> fileList = await PatchClient.GetFileList(this);

            if (fileList == default)
            {
                patchLocker.Set();
                DelayDownload(latestChangeTime);
                return;
            }

            //if (Info.LatestUpdate.HasValue != false)
            //    fileList = fileList.Where(x => x.LastChanged > Info.LatestUpdate.Value);

            //fileList = fileList.Where(x => !Info.IgnoreFilePaths.Any(ig => Regex.IsMatch(x.RelativePath, ig))).Reverse().ToList();

            fileList = fileList
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

            bool EOF = false;

            byte q = byte.MinValue;

            foreach (var file in fileList)
            {
                PatchClient.NextDownloadFile(file);
                StartFile(
                    PatchClient.Options.ClientData,
                    new Shared.Models.RequestModels.PublishFileStartRequestModel()
                    {
                        RelativePath = file.RelativePath,
                        CreateTime = file.CreationTime,
                        UpdateTime = file.ModifiedTime
                    });

                do
                {
                    await Task.Delay(20);
                    DownloadPacketData downloadProc = await PatchClient.Download();

                    if (downloadProc == default)
                    {
                        EndFile(PatchClient.Options.ClientData);
                        EndPatchReceive(false, latestChangeTime);
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
                EndPatchReceive(false, latestChangeTime);
                DelayDownload(latestChangeTime);
                return;
            }

            foreach (var item in result)
            {
                File.WriteAllBytes(Path.Combine(ProjectDirPath, item.fileName), item.data);
            }

            Info.LatestUpdate = latestChangeTime;

            getScript(true);

            EndPatchReceive(true, latestChangeTime);
        }

        private void EndPatchReceive(bool success, DateTime updateTime)
        {
            if (success)
            {
                try { runScriptOnStart(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                success = processTemp();

                if (!success)
                    recoveryBackup();
                try { runScriptOnEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }
            }

            processFileList.Clear();

            if (success)
            {
                DumpFileList();

                try { runScriptOnSuccessEnd(new Dictionary<string, string>()); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

                Info.LatestUpdate = updateTime;
                SaveProjectInfo();

                broadcastUpdateTime();
            }
            else
                try { runScriptOnFailedEnd(); } catch (Exception ex) { BroadcastMessage(ex.ToString()); }

            patchLocker.Set();
        }

        private async void DelayDownload(DateTime latestChangeTime)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));

            await Download(latestChangeTime);
        }
    }
}
