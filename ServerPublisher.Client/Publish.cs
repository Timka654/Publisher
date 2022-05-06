﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections.Concurrent;
using ServerPublisher.Shared;
using NSL.Cipher.RSA;
using Newtonsoft.Json;
using ServerPublisher.Client.Library;

namespace ServerPublisher.Client
{
    public class PublishInfo
    {
        public int BufferLen { get; set; } = 409088;

        public string PublishDirectory { get; set; }

        public string ProjectId { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; } = 6583;

        public string SuccessArgs { get; set; }

        public string AuthKeyPath { get; set; }

        public string InputKey { get; set; }

        public string OutputKey { get; set; }

        public bool HasCompression { get; set; } = false;
    }

    public class Publish
    {
        public static Publish Instance { get; set; }

        private CommandLineArgs successArgs = null;

        private Network network = null;

        private BasicUserInfo userInfo = null;

        private PublishInfo publishInfo;

        private List<string> ignorePatternList = null;

        private List<BasicFileInfo> uploadFileList = null;

        private List<BasicFileInfo> remoteFileList = null;

        internal void WriteLog(string v)
        {
            Console.WriteLine(v);
        }

        private T ReadConfigunation<T>(string basedir, string path, out string outBasePath)
        {
            outBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basedir, path);

            if (File.Exists(path))
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

            if (!File.Exists(outBasePath))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(outBasePath));
        }

        private void LogError(string text)
        {
            var def = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(text);

            Console.ForegroundColor = def;

            Environment.Exit(-1);
        }

        private void EmptyParameterError(string name)
        {
            LogError($"Run command must have \"{name}\" parameter");
        }

        public async Task Run(CommandLineArgs args)
        {
            Console.WriteLine("run");

            if (args.TryGetOutValue("configuration_path", out string configPath))
            {
                publishInfo = ReadConfigunation<PublishInfo>("configurations", configPath, out var cpath);

                if (publishInfo == default)
                {
                    LogError($"configuration_path file by path {cpath} or {configPath} not exists");
                }
            }
            else
            {
                publishInfo = new PublishInfo();
            }

            if (args.TryGetOutValue("project_id", out string projectId))
                publishInfo.ProjectId = projectId;

            if (args.TryGetOutValue("directory", out string publishDirectory))
                publishInfo.PublishDirectory = publishDirectory;

            if (args.TryGetOutValue("auth_key_path", out string authKeyPath))
                publishInfo.AuthKeyPath = authKeyPath;

            if (args.TryGetOutValue("cipher_out_key", out string inputKey))
                publishInfo.InputKey = inputKey;

            if (args.TryGetOutValue("cipher_in_key", out string outputKey))
                publishInfo.OutputKey = outputKey;

            if (args.TryGetOutValue("ip", out string ip))
                publishInfo.Ip = ip;

            if (args.TryGetOutValue("port", out int port))
                publishInfo.Port = port;

            if (args.TryGetOutValue("buffer_len", out int bufferLen))
                publishInfo.BufferLen = bufferLen - 512;

            if (args.TryGetOutValue("success_args", out string successArgs))
                publishInfo.SuccessArgs = successArgs;

            if (args.TryGetOutValue("has_compression", out bool hasCompression))
                publishInfo.HasCompression = hasCompression;

            if (publishInfo.ProjectId == default)
                EmptyParameterError("project_id");

            if (publishInfo.PublishDirectory == default)
                EmptyParameterError("directory");

            if (publishInfo.Ip == default)
                EmptyParameterError("ip");

            if (publishInfo.AuthKeyPath == default)
                EmptyParameterError("auth_key_path");

            if (publishInfo.InputKey == default)
                EmptyParameterError("cipher_out_key");

            if (publishInfo.OutputKey == default)
                EmptyParameterError("cipher_in_key");

            if (publishInfo.Port == default)
                EmptyParameterError("port");

            if (publishInfo.BufferLen == default)
                EmptyParameterError("buffer_len");

            if (publishInfo.SuccessArgs == default)
                this.successArgs = new CommandLineArgs(new string[] { });
            else
                this.successArgs = new CommandLineArgs(publishInfo.SuccessArgs.Split(" /").Select(x => "/" + x).ToArray());

            if (!Directory.Exists(publishInfo.PublishDirectory))
                LogError($"Publish directory {publishInfo.PublishDirectory} not exists");

            userInfo = ReadConfigunation<BasicUserInfo>("key_storage", publishInfo.AuthKeyPath, out string keyPath);

            if (userInfo == default)
            {
                LogError($"configuration_path file by path {keyPath} or {publishInfo.AuthKeyPath} not exists");
            }

            network = new Network(publishInfo.Ip, publishInfo.Port, publishInfo.InputKey, publishInfo.OutputKey, (e) => { if (finished) return; LogError("Server disconnected!!"); StepLocker.Set(); });

            Console.WriteLine($"Try connect to {publishInfo.Ip}:{publishInfo.Port}");

            if (!network.Connect())
            {
                LogError($"Cannot connect");
            }

            Console.WriteLine($"Success connected");

            network.OnProjectPublishStartMessage += ProjectPublishStart_OnReceiveEvent;
            network.OnServerLogMessage += Instance_OnReceiveEvent;

            Console.WriteLine($"Try sign");

            var result = await SignIn();

            if (result != SignStateEnum.Ok)
            {
                LogError($"Sign result - {Enum.GetName<SignStateEnum>(result)}, error");
            }
            Console.WriteLine($"Sign result - ok, wait....");


            StepLocker.WaitOne();
        }

        private void Instance_OnReceiveEvent(string value)
        {
            Console.WriteLine(value);
        }

        private async void ProjectPublishStart_OnReceiveEvent(List<string> value)
        {
            Console.WriteLine($"PublishStart, build ignore pattern list");
            for (int i = 0; i < value.Count; i++)
            {
                value[i] = value[i].Replace("**", "[\\s|\\S]");

                Console.WriteLine($"[{i + 1}]-{value[i]}");
            }

            ignorePatternList = value;

            Console.WriteLine($"Ignore pattern builded");

            Console.WriteLine($"Build file list");

            uploadFileList = GetFiles(publishInfo.PublishDirectory);

            remoteFileList = await network.GetFileList();

            if (remoteFileList == default)
                LogError($"Cannot receive file list");

            uploadFileList.RemoveAll(x => remoteFileList.Any(r => r.RelativePath == x.RelativePath && r.Hash == x.Hash));

            Console.WriteLine($"Start upload - {uploadFileList.Count} files");

            await UploadFiles();

            StepLocker.Set();
        }

        private System.Threading.AutoResetEvent StepLocker = new System.Threading.AutoResetEvent(false);
        bool finished = false;

        private async Task UploadFiles()
        {
            if (uploadFileList.Any())
            {
                if (publishInfo.HasCompression)
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

                    await UploadFile(new BasicFileInfo(archiveFileInfo.Directory.FullName, archiveFileInfo), true);
                }
                else
                {
                    foreach (var item in uploadFileList)
                    {
                        await UploadFile(item);
                    }
                }
            }

            await network.ProjectPublishEnd(successArgs);
            finished = true;
            network.Disconnect();

            Environment.Exit(0);
        }

        private async Task UploadFile(BasicFileInfo file, bool compressed = false)
        {
            await network.FilePublishStart(file);

            var fs = file.FileInfo.OpenRead();

            byte[] buf = new byte[publishInfo.BufferLen];

            int currLen = default;

            do
            {
                currLen = fs.Read(buf, 0, publishInfo.BufferLen);

                if (currLen == 0)
                    break;

                Console.WriteLine($"Uploading: {100.0 / fs.Length * fs.Position}%");

                await network.UploadFileBytes(buf, currLen);

            } while (currLen == publishInfo.BufferLen);

            fs.Close();
        }

        private List<BasicFileInfo> GetFiles(string dir)
        {
            ConcurrentBag<BasicFileInfo> result = new ConcurrentBag<BasicFileInfo>();

            var fileList = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

            Parallel.ForEach(fileList,
                item =>
            {
                if (ignorePatternList.Any(x => Regex.IsMatch(Path.GetRelativePath(dir, item), x)))
                    return;

                BasicFileInfo temp = new BasicFileInfo(dir, new FileInfo(item));

                temp.CalculateHash();

                Console.WriteLine($"Calculate local hash: {temp.RelativePath} - {temp.Hash}");

                result.Add(temp);
            });

            return result.ToList();
        }

        private async Task<SignStateEnum> SignIn()
        {
            RSACipher rsa = new RSACipher();
            rsa.LoadXml(userInfo.RSAPublicKey);

            var temp = Encoding.ASCII.GetBytes(userInfo.Id);
            temp = rsa.Encode(temp, 0, temp.Length);

            return await network.SignIn(publishInfo.ProjectId, userInfo, temp, publishInfo.HasCompression);
        }
    }
}