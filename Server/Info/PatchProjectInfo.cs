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
    public partial class ServerProjectInfo
    {
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
                client.PatchProjectMap = new Dictionary<string, ServerProjectInfo>();
            }
            else
            {
                if (client.PatchProjectMap.ContainsKey(Info.Id))
                {
                    if (Info.LatestUpdate.HasValue && Info.LatestUpdate.Value > latestUpdate)
                        broadcastUpdateTime(client);

                    return SignStateEnum.Ok;
                }
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

            initializeLogger();
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

            initializeLogger();
            initializeTemp();

            IEnumerable<BasicFileInfo> fileList = await PatchClient.GetFileList(this);

            if (fileList == default)
            {
                patchLocker.Set();
                DelayDownload(latestChangeTime);
                return;
            }

            //if (Info.LatestUpdate.HasValue != false)
            //    fileList = fileList.Where(x => x.LastChanged > Info.LatestUpdate.Value);

            //fileList = fileList.Where(x => !Info.IgnoreFilePaths.Any(ig => Regex.IsMatch(x.RelativePath, ig))).Reverse().ToList();

            fileList = fileList.Where(x => !Info.IgnoreFilePaths.Any(ig => Regex.IsMatch(x.RelativePath, ig))).Where(x =>
            {
                x.FileInfo = new FileInfo(Path.Combine(ProjectDirPath, x.RelativePath));

                if (x.FileInfo.Exists == false)
                    return true;
                string remote = x.Hash;
                x.CalculateHash();
                return x.Hash != remote;
            }).Reverse().ToList();

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

                Info.LatestUpdate = DateTime.UtcNow;
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
