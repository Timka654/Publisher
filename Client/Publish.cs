using Cipher.RSA;
using Publisher.Basic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace Publisher.Client
{
    public class PublishInfo
    {
        public int bufferLen = 40960;
        public string publishDirectory = default;
        public string projectId = default;
        public string ip = default;
        public int port = 6583;
        public string successArgs = default;
        public string authKeyPath = default;
        public string inputKey = default;
        public string outputKey = default;

        public int BufferLen { get => bufferLen; set => bufferLen = value; }
        public string PublishDirectory { get => publishDirectory; set => publishDirectory = value; }
        public string ProjectId { get => projectId; set => projectId = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string SuccessArgs { get => successArgs; set => successArgs = value; }
        public string AuthKeyPath { get => authKeyPath; set => authKeyPath = value; }
        public string InputKey { get => inputKey; set => inputKey = value; }
        public string OutputKey { get => outputKey; set => outputKey = value; }
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

            string configPath = "";

            if (args.TryGetValue("configuration_path", ref configPath))
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

            args.TryGetValue("project_id", ref publishInfo.projectId);
            args.TryGetValue("directory", ref publishInfo.publishDirectory);

            args.TryGetValue("auth_key_path", ref publishInfo.authKeyPath);
            args.TryGetValue("cipher_out_key", ref publishInfo.inputKey);
            args.TryGetValue("cipher_in_key", ref publishInfo.outputKey);

            args.TryGetValue("ip", ref publishInfo.ip);
            args.TryGetValue("port", ref publishInfo.port);
            args.TryGetValue("buffer_len", ref publishInfo.bufferLen);
            args.TryGetValue("success_args", ref publishInfo.successArgs);

            if (publishInfo.projectId == default)
                EmptyParameterError("project_id");

            if (publishInfo.publishDirectory == default)
                EmptyParameterError("directory");

            if (publishInfo.ip == default)
                EmptyParameterError("ip");

            if (publishInfo.authKeyPath == default)
                EmptyParameterError("auth_key_path");

            if (publishInfo.inputKey == default)
                EmptyParameterError("cipher_out_key");

            if (publishInfo.outputKey == default)
                EmptyParameterError("cipher_in_key");

            if (publishInfo.port == default)
                EmptyParameterError("port");

            if (publishInfo.bufferLen == default)
                EmptyParameterError("buffer_len");

            if (publishInfo.successArgs == default)
                successArgs = new CommandLineArgs(new string[] { });
            else
                successArgs = new CommandLineArgs(publishInfo.successArgs.Split(" /").Select(x => "/" + x).ToArray());

            if (!Directory.Exists(publishInfo.publishDirectory))
                LogError($"Publish directory {publishInfo.publishDirectory} not exists");

            userInfo = ReadConfigunation<BasicUserInfo>("key_storage", publishInfo.authKeyPath, out string keyPath);

            if (userInfo == default)
            {
                LogError($"configuration_path file by path {keyPath} or {publishInfo.authKeyPath} not exists");
            }

            network = new Network(publishInfo.ip, publishInfo.port, publishInfo.inputKey, publishInfo.outputKey, (e)=> { if (finished) return; LogError("Server disconnected!!"); StepLocker.Set(); });

            Console.WriteLine($"Try connect to {publishInfo.ip}:{publishInfo.port}");

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

                Console.WriteLine($"[{i+1}]-{value[i]}");
            }

            ignorePatternList = value;

            Console.WriteLine($"Ignore pattern builded");

            Console.WriteLine($"Build file list");

            uploadFileList = GetFiles(publishInfo.publishDirectory);

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
            int currLen = publishInfo.bufferLen;
            FileStream fs = null;

            byte[] buf = new byte[publishInfo.bufferLen];

            foreach (var item in uploadFileList)
            {
                currLen = publishInfo.bufferLen;

                await network.FilePublishStart(item);

                fs = item.FileInfo.OpenRead();

                do
                {
                    currLen = fs.Read(buf, 0, publishInfo.bufferLen);

                    if (currLen == 0)
                        break;

                    await network.UploadFileBytes(buf, currLen);

                } while (currLen == publishInfo.bufferLen);

                fs.Close();

            }

            await network.ProjectPublishEnd(successArgs);
            finished = true;
            network.Disconnect();

            Environment.Exit(0);
        }

        private List<BasicFileInfo> GetFiles(string dir)
        {
            List<BasicFileInfo> result = new List<BasicFileInfo>();

            BasicFileInfo temp;

            foreach (var item in Directory.GetFiles(dir,"*", SearchOption.AllDirectories))
            {
                if (ignorePatternList.Any(x => Regex.IsMatch(Path.GetRelativePath(publishInfo.publishDirectory, item), x)))
                    continue;

                temp = new BasicFileInfo(publishInfo.publishDirectory, new FileInfo(item));

                temp.CalculateHash();

                Console.WriteLine($"Calculate local hash: {temp.RelativePath} - {temp.Hash}");

                result.Add(temp);
            }

            return result;
        }


        private async Task<SignStateEnum> SignIn()
        {
            RSACipher rsa = new RSACipher();
            rsa.LoadXml(userInfo.RSAPublicKey);

            var temp = Encoding.ASCII.GetBytes(userInfo.Id);
            temp = rsa.Encode(temp, 0, temp.Length);

            return await network.SignIn(publishInfo.projectId, userInfo, temp);
        }
    }
}
