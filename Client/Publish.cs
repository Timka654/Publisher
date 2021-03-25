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

namespace Publisher.Client
{
    public class Publish
    {
        private readonly int bufLen = 40960;

        public static Publish Instance { get; set; }

        private string publishDirectory = "";

        private string projectId = "";

        private CommandLineArgs successArgs = null;

        private Network network = null;

        private BasicUserInfo userInfo = null;
        private List<string> ignorePatternList = null;

        private List<BasicFileInfo> uploadFileList = null;

        private List<BasicFileInfo> remoteFileList = null;

        internal void WriteLog(string v)
        {
            StaticInstances.ServerLogger.AppendInfo(v);
        }

        public async Task Run(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("run");

            if (!args.ContainsKey("project_id"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"project_id\" parameter");
                Environment.Exit(0);
            }

            if (!args.ContainsKey("directory"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"directory\" parameter");
                Environment.Exit(0);
            }

            if (!args.ContainsKey("ip"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"ip\" parameter");
                Environment.Exit(0);
            }

            if (!args.ContainsKey("auth_key_path"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"auth_key_path\" parameter");
                Environment.Exit(0);
            }

            if (!args.ContainsKey("cipher_out_key"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"cipher.out.key\" parameter");
                Environment.Exit(0);
            }

            if (!args.ContainsKey("cipher_in_key"))
            {
                StaticInstances.ServerLogger.AppendError($"Run command must have \"cipher.in.key\" parameter");
                Environment.Exit(0);
            }

            publishDirectory = args["directory"];

            if (!Directory.Exists(publishDirectory))
            {
                StaticInstances.ServerLogger.AppendError($"Publish directory {publishDirectory} not exists");
                Environment.Exit(0);
            }

            if (args.ContainsKey("success_args"))
                successArgs = new CommandLineArgs(args["success_args"].Split(" /").Select(x=>"/" + x).ToArray());
            else
                successArgs = new CommandLineArgs(new string[] { });


            var ip = args["ip"];
            var port = args.ContainsKey("port") ? Convert.ToInt32(args["port"]) : 6583;

            var authKeyPath = args["auth_key_path"];

            if (!File.Exists(authKeyPath))
            {
                var temp_path = authKeyPath;
                authKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key_storage", authKeyPath);

                if (!File.Exists(authKeyPath))
                {
                    StaticInstances.ServerLogger.AppendError($"Auth key file {temp_path} or {authKeyPath} not exists");
                    Environment.Exit(0);
                }
            }

            var inputKey = args["cipher_out_key"];
            var outputKey = args["cipher_in_key"];

            projectId = args["project_id"];

            network = new Network(ip, port, inputKey, outputKey);

            if (!network.Connect()){

                StaticInstances.ServerLogger.AppendInfo($"cannot connect to {ip}:{port}");
                Environment.Exit(0);
            }

            network.OnProjectPublishStartMessage += ProjectPublishStart_OnReceiveEvent;
            network.OnServerLogMessage += Instance_OnReceiveEvent;

            userInfo = JsonSerializer.Deserialize<BasicUserInfo>(File.ReadAllText(authKeyPath), options: new JsonSerializerOptions() { IgnoreNullValues = true, IgnoreReadOnlyProperties = true, });

            var result = await SignIn();

            if (result != SignStateEnum.Ok)
            {
                StaticInstances.ServerLogger.AppendInfo($"sign result {Enum.GetName(typeof(SignStateEnum),result)}, error");
            }
            
            StepLocker.WaitOne();

            uploadFileList = GetFiles(publishDirectory).Select(x => new BasicFileInfo(publishDirectory, x)).ToList();

            foreach (var item in uploadFileList)
            {
                item.CalculateHash();
            }

            remoteFileList = await network.GetFileList();

            uploadFileList.RemoveAll(x => remoteFileList.Any(r => r.RelativePath == x.RelativePath && r.Hash == x.Hash));

            await UploadFiles();
        }

        private void Instance_OnReceiveEvent(string value)
        {
            StaticInstances.ServerLogger.AppendInfo(value);
        }

        private void ProjectPublishStart_OnReceiveEvent(List<string> value)
        {
            ignorePatternList = value;
            StepLocker.Set();
        }

        private System.Threading.AutoResetEvent StepLocker = new System.Threading.AutoResetEvent(false);

        private async Task UploadFiles()
        {
            int currLen = bufLen;
            FileStream fs = null;

            byte[] buf = new byte[ bufLen];

            foreach (var item in uploadFileList)
            {
                currLen = bufLen;

                await network.FilePublishStart(item);

                fs = item.FileInfo.OpenRead();

                do
                {
                    currLen = fs.Read(buf, 0, bufLen);

                    if (currLen == 0)
                        break;

                    await network.UploadFileBytes(buf, currLen);

                } while (currLen == bufLen);

                fs.Close();

            }

            await network.ProjectPublishEnd(successArgs);

            network.Disconnect();
            //await Task.Delay(15000);

            Environment.Exit(0);
        }

        private List<FileInfo> GetFiles(string dir)
        {
            List<FileInfo> result = new List<FileInfo>();

            foreach (var item in Directory.GetFiles(dir))
            {
                if (ignorePatternList.Any(x => Regex.IsMatch(Path.GetRelativePath(publishDirectory, item), x)))
                    continue;

                result.Add(new FileInfo(item));
            }

            foreach (var item in Directory.GetDirectories(dir))
            {
                result.AddRange(GetFiles(item));
            }

            return result;
        }


        private async Task<SignStateEnum> SignIn()
        {
            RSACipher rsa = new RSACipher();
            rsa.LoadXml(userInfo.RSAPublicKey);

            var temp = Encoding.ASCII.GetBytes(userInfo.Id);
            temp = rsa.Encode(temp,0, temp.Length);

           return await network.SignIn(projectId, userInfo, temp);
        }
    }
}
