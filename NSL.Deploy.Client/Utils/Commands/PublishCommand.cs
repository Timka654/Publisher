using Newtonsoft.Json;
using NSL.Deploy.Client.Deploy;
using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Info;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("config_path", typeof(string), true)]
    [CLArgument("project_id", typeof(string), true)]
    [CLArgument("output_relative_path", typeof(string), true)]
    [CLArgument("directory", typeof(string), true)]
    [CLArgument("auth_key_path", typeof(string), true)]
    [CLArgument("cipher_out_key", typeof(string), true)]
    [CLArgument("cipher_in_key", typeof(string), true)]
    [CLArgument("ip", typeof(string), true)]
    [CLArgument("port", typeof(int), true)]
    [CLArgument("buffer_len", typeof(int), true)]
    [CLArgument("success_args", typeof(string), true)]
    [CLArgument("has_compression", typeof(bool), true)]
    internal partial class PublishCommand : CLHandler
    {
        public override string Command => "publish";

        public override string Description { get => "Publish project to server"; set => base.Description = value; }

        public PublishCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("config_path")] private string ConfigurationPath { get; set; }

        [CLArgumentValue("output_relative_path")] private string OutputRelativePath { get; set; }

        [CLArgumentValue("project_id")] private string ProjectId { get; set; }

        [CLArgumentValue("directory")] private string PublishDirectory { get; set; }

        [CLArgumentValue("auth_key_path")] private string AuthKeyPath { get; set; }

        [CLArgumentValue("cipher_out_key")] private string InputKey { get; set; }

        [CLArgumentValue("cipher_in_key")] private string OutputKey { get; set; }

        [CLArgumentValue("ip")] private string Ip { get; set; }

        [CLArgumentValue("port")] private int Port { get; set; }

        [CLArgumentValue("buffer_len")] private int BufferLen { get; set; }

        [CLArgumentValue("success_args")] private string successArgs { get; set; }

        [CLArgumentValue("has_compression")] private bool HasCompression { get; set; }


        [CLArgumentExists("config_path")] private bool ConfigurationPathExists { get; set; }

        [CLArgumentExists("output_relative_path")] private bool OutputRelativePathExists { get; set; }

        [CLArgumentExists("project_id")] private bool ProjectIdExists { get; set; }

        [CLArgumentExists("directory")] private bool PublishDirectoryExists { get; set; }

        [CLArgumentExists("auth_key_path")] private bool AuthKeyPathExists { get; set; }

        [CLArgumentExists("cipher_out_key")] private bool InputKeyExists { get; set; }

        [CLArgumentExists("cipher_in_key")] private bool OutputKeyExists { get; set; }

        [CLArgumentExists("ip")] private bool IpExists { get; set; }

        [CLArgumentExists("port")] private bool PortExists { get; set; }

        [CLArgumentExists("buffer_len")] private bool BufferLenExists { get; set; }

        [CLArgumentExists("success_args")] private bool SuccessArgsExists { get; set; }

        [CLArgumentExists("has_compression")] private bool HasCompressionExists { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

#if DEBUG
            var di = new DirectoryInfo(PublishDirectory);

            if (!di.Exists)
                di.Create();

            if (!di.GetFiles().Any())
            {
                for (int i = 0; i < 3; i++)
                {
                    using var f = File.OpenWrite(Path.Combine(di.FullName, $"{i}.bin"));
                    byte[] temp = new byte[100 * 1024 * 1024];

                    Random.Shared.NextBytes(temp);

                    f.Write(temp);
                }

            }
#endif


            ProjectInfo publishInfo = default;


            if (ConfigurationPathExists)
            {
                publishInfo = ReadConfiguration<ProjectInfo>("configurations", ConfigurationPath, out var baseConfigurationPath, out var configurationPath);

                if (publishInfo == default)
                {
                    AppCommands.Logger.AppendError($"Configuration file by path \"{configurationPath}\"/\"{baseConfigurationPath}\" not found");

                    return CommandReadStateEnum.Failed;
                }
            }
            else
            {
                publishInfo = new ProjectInfo();
            }

            if (ProjectIdExists)
                publishInfo.ProjectId = ProjectId;

            if (PublishDirectoryExists)
                publishInfo.PublishDirectory = PublishDirectory;

            if (OutputRelativePathExists && !string.IsNullOrWhiteSpace(OutputRelativePath))
                publishInfo.OutputRelativePath = OutputRelativePath;

            if (InputKeyExists)
                publishInfo.InputKey = InputKey;

            if (OutputKeyExists)
                publishInfo.OutputKey = OutputKey;

            if (IpExists)
                publishInfo.Ip = Ip;

            if (PortExists)
                publishInfo.Port = Port;

            if (BufferLenExists)
                publishInfo.BufferLen = BufferLen - 512;

            if (HasCompressionExists)
                publishInfo.HasCompression = HasCompression;


            if (SuccessArgsExists)
                publishInfo.SuccessArgs = new CommandLineArgs(successArgs.Split(" /").Select(x => "/" + x).ToArray(), false).GetArgs().ToDictionary(x => x.Key, x => x.Value);

            if (!Directory.Exists(publishInfo.PublishDirectory))
                AppCommands.Logger.AppendError($"Publish directory {publishInfo.PublishDirectory} not exists");

            publishInfo.Identity = ReadConfiguration<BasicUserInfo>(Program.KeysPath, AuthKeyPath, out string baseIdentityPath, out string identityPath);

            if (publishInfo.Identity == default)
            {
                AppCommands.Logger.AppendError($"Identity file by path \"{identityPath}\"/\"{baseIdentityPath}\" not found");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.ProjectId == default)
            {
                EmptyParameterError("project_id");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.PublishDirectory == default)
            {
                EmptyParameterError("directory");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.Ip == default)
            {
                EmptyParameterError("ip");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.InputKey == default)
            {
                EmptyParameterError("cipher_out_key");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.OutputKey == default)
            {
                EmptyParameterError("cipher_in_key");
                return CommandReadStateEnum.Failed;
            }

            if (publishInfo.BufferLen == default)
            {
                EmptyParameterError("buffer_len");
                return CommandReadStateEnum.Failed;
            }

            await new DeployClient()
            {
                OnCannotConnected = () => AppCommands.Logger.AppendError($"Cannot connect"),
                OnConnectionLost = () => AppCommands.Logger.AppendError("Connection lost"),
                OnCannotSignIn = state => AppCommands.Logger.AppendError($"Cannot sign - {state.ToString()}"),
                PublishInfo = publishInfo
            }
            .Publish();

            return CommandReadStateEnum.Success;
        }


        private void EmptyParameterError(string name)
            => AppCommands.Logger.AppendError($"Run command must have \"{name}\" parameter");

        private T ReadConfiguration<T>(string baseDir, string path, out string outBasePath, out string outPath)
        {
            outBasePath = Path.Combine(baseDir, path);

            var result = ReadConfiguration<T>(path, out outPath);

            if (result != null)
                return result;

            return ReadConfiguration<T>(outBasePath, out outBasePath);
        }

        private T ReadConfiguration<T>(string path, out string outFullPath)
        {
            outFullPath = Path.GetFullPath(path, Directory.GetCurrentDirectory());

            if (File.Exists(path))
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

            return default;
        }
    }
}
