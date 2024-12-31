using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections.Concurrent;
using NSL.Cipher.RSA;
using ServerPublisher.Client.Library;
using NSL.Utils;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Enums;
using ServerPublisher.Shared.Models.ResponseModel;
using System.Threading;
using ServerPublisher.Shared.Models.RequestModels;
using ServerPublisher.Shared.Utils;
using System.Diagnostics;

namespace NSL.Deploy.Client.Deploy
{
    public class DeployClient
    {
        private Network network = null;

        public required ProjectInfo PublishInfo { get; init; }

        private List<string> ignorePatternList = null;

        private List<BasicFileInfo> uploadFileList = null;

        private BasicFileInfo[] remoteFileList = null;

        public Action OnConnectionLost { get; set; }

        public Action OnCannotConnected { get; set; }

        public Action<SignStateEnum> OnCannotSignIn { get; set; }

        public async Task<bool> Publish()
        {
            network = new Network(PublishInfo.Ip, PublishInfo.Port, PublishInfo.InputKey, PublishInfo.OutputKey, (e) =>
            {
                if (finished) return;

                if (OnConnectionLost != null)
                    OnConnectionLost();

                StepLocker.Set();
            });

            Console.WriteLine($"Try connect to {PublishInfo.Ip}:{PublishInfo.Port}");

            if (!await network.ConnectAsync())
            {
                if (OnCannotConnected != null)
                    OnCannotConnected();

                return false;
            }

            Console.WriteLine($"Success connected");

            network.OnPublishProjectStartMessage += PublishProjectStartMessage_OnReceiveEvent;
            network.OnServerLogMessage += Instance_OnReceiveEvent;
            network.OnUploadPartMessage += Network_OnUploadPartMessage;

            Console.WriteLine($"Try sign");

            var result = await SignIn();

            if (result.Result != SignStateEnum.Ok)
            {
                if (OnCannotSignIn != null)
                    OnCannotSignIn(result.Result);

                return false;
            }

            if (startDelayToken.IsCancellationRequested)
                Console.WriteLine($"Sign result - ok");
            else
                Console.WriteLine($"Sign result - ok, wait....");

            try { await Task.Delay(-1, startDelayToken.Token); } catch { }

            var value = result.IgnoreFilePatterns;

            Console.WriteLine($"PublishStart, build ignore pattern list");
            for (int i = 0; i < value.Count; i++)
            {
                value[i] = value[i].Replace("**", "[\\s|\\S]");

                Console.WriteLine($"[{i + 1}]-{value[i]}");
            }

            ignorePatternList = value;

            Console.WriteLine($"Ignore pattern builded");

            Console.WriteLine($"Build file list");

            uploadFileList = GetFiles(PublishInfo.PublishDirectory);

            remoteFileList = fileList.FileList;

            uploadFileList.RemoveAll(x => remoteFileList.Any(r => r.RelativePath == x.RelativePath && r.Hash == x.Hash));

            Console.WriteLine($"Start upload - {uploadFileList.Count} files");

            await UploadFiles();

            StepLocker.Set();

            return true;
        }

        private void Network_OnUploadPartMessage(int len)
        {
            Interlocked.Add(ref uploadedLen, len);
            Interlocked.Decrement(ref uploadingCount);

            if (uploadedLen == uploadLen)
                uploadLocker.Set();

        }

        ProjectFileListResponseModel fileList;

        CancellationTokenSource startDelayToken = new CancellationTokenSource();

        private void Instance_OnReceiveEvent(string value)
        {
            logOutput.LineLog(value);
        }

        private async void PublishProjectStartMessage_OnReceiveEvent(ProjectFileListResponseModel value)
        {
            fileList = value;

            startDelayToken.Cancel();
        }

        private AutoResetEvent StepLocker = new AutoResetEvent(false);
        bool finished = false;

        private async Task UploadFiles()
        {
            if (uploadFileList.Any())
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                var token = tokenSource.Token;

                statsOutput(token);

                if (PublishInfo.HasCompression)
                {
                    var archivePath = Path.GetTempFileName();

                    using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
                    {
                        foreach (var item in uploadFileList)
                        {
                            archive.CreateEntryFromFile(item.FileInfo.FullName, item.RelativePath, CompressionLevel.Fastest);
                        }
                    }

                    var archiveFileInfo = new FileInfo(archivePath);

                    uploadLen = archiveFileInfo.Length;

                    await UploadFile(new BasicFileInfo(archiveFileInfo.Directory.GetNormalizedDirectoryPath(), archiveFileInfo), token, true);
                }
                else
                {
                    uploadLen = uploadFileList.Sum(x => x.FileInfo.Length);

                    //await Parallel.ForEachAsync(uploadFileList, async (item, token) =>
                    //{
                    foreach (var item in uploadFileList)
                    {
                        await UploadFile(item, token);
                    }
                    //});
                }


                await Task.WhenAll(waitUploadTasks.ToArray());

                uploadLocker.WaitOne();

                displayStats(null);

                tokenSource.Cancel();

            }

            var result = await network.ProjectFinish(new PublishProjectFinishRequestModel()
            {
                Args = PublishInfo.SuccessArgs
            });

            if (result)
            {
                finished = true;
                network.Disconnect();
                Environment.Exit(0);
            }
        }

        long uploadLen = 0;
        long uploadedLen = 0;

        long uploadingCount = 0;

        List<long> uploadedMarks = new List<long>();

        ConcurrentBag<Task> waitUploadTasks = new ConcurrentBag<Task>();

        ManualResetEvent uploadLocker = new ManualResetEvent(false);

        DateTime startTime;

        private async void statsOutput(CancellationToken token)
        {
            try
            {
                int i = 0;
                long speed = 0;
                startTime = DateTime.UtcNow.AddSeconds(1);
                while (true)
                {
                    var c = uploadedLen;
                    await Task.Delay(1000, token);

                    if (token.IsCancellationRequested)
                        return;

                    var old = c;

                    c = uploadedLen;

                    ++i;

                    if (c != old)
                    {

                        speed = (c - old) / i;

                        i = 0;

                        uploadedMarks.Add(speed);
                    }

                    displayStats(speed);
                }
            }
            catch (Exception)
            {
            }
        }

        private NSLConsoleOutput logOutput => NSLConsoleOutput.Instance;

        private void displayStats(long? speed)
        {
            //outputLocker.WaitOne();

            //Console.SetCursorPosition(0, Console.CursorTop - 1);

            var t = $"Uploaded {uploadedLen / 1024:N0}/{uploadLen / 1024:N0} kbytes {1.0 * uploadedLen / uploadLen:P}. Speed {speed / 1024:N2}(Max: {uploadedMarks.DefaultIfEmpty()?.Max() / 1024:N2}, Avg: {uploadedMarks.DefaultIfEmpty()?.Average() / 1024:N2}, Min: {uploadedMarks.DefaultIfEmpty()?.Min() / 1024:N2}) kbytes/s, {DateTime.UtcNow - startTime:hh\\:mm\\:ss}";

            logOutput.ReplaceLog(t, 1);

            //int currentLineCursor = Console.CursorTop;
            //Console.SetCursorPosition(0, Console.CursorTop);
            //Console.WriteLine(t.PadRight(consoleWidth - 1));
            //Console.SetCursorPosition(0, currentLineCursor);

            //outputLocker.Set();
        }

        private int uploadBufferLen => PublishInfo.BufferLen;

        private SemaphoreSlim uploadBalancing = new SemaphoreSlim(20000);

        private async Task UploadFile(BasicFileInfo file, CancellationToken cancellationToken, bool compressed = false)
        {
            //if (file.RelativePath != "n1.bin")
            //    return;

            var fsr = await network.FileStart(new PublishProjectFileStartRequestModel()
            {
                RelativePath = file.RelativePath,
                Length = file.FileInfo.Length,
                CreateTime = file.FileInfo.CreationTime,
                UpdateTime = file.FileInfo.LastWriteTime
            });

            var fs = file.FileInfo.OpenRead();

            //int len = uploadBufferLen;

            //ConcurrentBag<Task> temp = new();

            //SemaphoreSlim locker = new(1);

            var cnt = fs.Length / uploadBufferLen;

            if (fs.Length > cnt * uploadBufferLen)
                ++cnt;

            ConcurrentBag<long> readInvoke = new ConcurrentBag<long>();
            ConcurrentBag<long> uploadInvoke = new ConcurrentBag<long>();

            ConcurrentBag<Task> tasks = new();

            for (int _i = 0; _i < cnt; _i++)
            {
                var i = _i;
                //var t = Task.Run(async () =>
                //{
                //    await uploadBalancing.WaitAsync();

                byte[] buf = new byte[uploadBufferLen];

                int currLen = default;

                var r = Stopwatch.StartNew();

                //await locker.WaitAsync();

                var offset = fs.Position = i * uploadBufferLen;

                currLen = fs.Read(buf, 0, buf.Length);

                //locker.Release();

                readInvoke.Add(r.ElapsedMilliseconds);
                //Console.WriteLine($"Read {readInvoke.Min()}/{readInvoke.Average()}/{readInvoke.Max()}");

                if (currLen < buf.Length)
                    Array.Resize(ref buf, currLen);

                //Console.WriteLine($"Uploading {file.RelativePath}: {100.0 / fs.Length * fs.Position}%");


                var u = Stopwatch.StartNew();
                Interlocked.Increment(ref uploadingCount);

                await network.UploadFilePart(new PublishProjectUploadFileBytesRequestModel()
                {
                    Bytes = buf,
                    Offset = offset,
                    FileId = fsr.FileId
                });

                uploadInvoke.Add(u.ElapsedMilliseconds);
                //Console.WriteLine($"Upload {uploadInvoke.Min()}/{uploadInvoke.Average()}/{uploadInvoke.Max()}");

                //uploadBalancing.Release();

                //Interlocked.Decrement(ref uploadingCount);

                //Interlocked.Add(ref uploadedLen, currLen);


                //}).ContinueWith(t =>
                //{
                //    if (!t.IsCompletedSuccessfully)
                //        return;

                //});

                //waitUploadTasks.Add(t);
                //tasks.Add(t);
            }

            await Task.WhenAll(tasks);

            //Console.WriteLine($"Uploaded file {file.RelativePath}");

            //Task.WhenAll(waitUploadTasks).ContinueWith(t => { 
            fs.Close();
            //}).RunAsync();
        }

        private List<BasicFileInfo> GetFiles(string dir)
        {
            ConcurrentBag<BasicFileInfo> result = new ConcurrentBag<BasicFileInfo>();

            var fileList = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

            Parallel.ForEach(fileList,
                item =>
            {
                if (ignorePatternList.Any(x => Regex.IsMatch(Path.GetRelativePath(dir, item).GetNormalizedPath(), x)))
                    return;

                BasicFileInfo temp = new BasicFileInfo(dir, new FileInfo(item));

                temp.CalculateHash();

                Console.WriteLine($"Calculate local hash: {temp.RelativePath} - {temp.Hash}");

                result.Add(temp);
            });

            return result.ToList();
        }

        private async Task<PublishSignInResponseModel> SignIn()
        {
            RSACipher rsa = new RSACipher();
            rsa.LoadXml(PublishInfo.Identity.RSAPublicKey);

            var temp = Encoding.ASCII.GetBytes(PublishInfo.Identity.Id);
            temp = rsa.Encode(temp, 0, temp.Length);

            var request = new PublishSignInRequestModel()
            {
                ProjectId = PublishInfo.ProjectId,
                UserId = PublishInfo.Identity.Id,
                IdentityKey = temp,
                UploadMethod = PublishInfo.HasCompression ? UploadMethodEnum.SingleArchive : UploadMethodEnum.Default
            };

            return await network.SignIn(request);
        }
    }
}
