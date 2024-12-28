using NSL.Logger;
using NSL.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("default", typeof(CLContainsType), true, Description = "Execute command with default parameters")]
    [CLArgument("path", typeof(string), true, Description = "Directory path for install")]
    [CLArgument("q", typeof(CLContainsType), true, Description = "Close console after install")]
    internal class InstallCommand : CLHandler
    {
        public override string Command => "install";

        public override string Description { get => "Command for install or update app, configure for integrate app in your os for fast execute"; set => base.Description = value; }

        public InstallCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentExists("q")] private bool quit { get; set; }

        [CLArgumentExists("path")] private bool containsPathArg { get; set; }

        [CLArgumentValue("path")] private string path { get; set; }

        [CLArgumentValue("default")] private bool defaultConfiguration { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            if (PermissionUtils.RequireRunningAsAdministrator())
                return CommandReadStateEnum.Success;

            ProcessingAutoArgs(values);

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            var path = this.path;

            if (!containsPathArg)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    path = "/etc/nsldeployclient";
                }
                else
                {
                    //if (Environment.Is64BitProcess)
                    //    path = @"C:\Program Files (x86)\Publisher.Client";
                    //else
                    path = @"C:\Program Files\NSL.Deploy.Client";
                }

                if (!defaultConfiguration)
                    path = CommandParameterReader.Read("Install directory", AppCommands.Logger, path);
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            IOUtils.CopyDirectory(appPath, path, true, filter: (targetFilePath, file) =>
            {

                AppCommands.Logger.AppendInfo($"Copy from {file.FullName} to {targetFilePath}");

                return true;
            });

            AppCommands.InitData(path);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                foreach (var item in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    FileSystemAccessRule rule = new("Everyone", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                    new FileInfo(item).GetAccessControl().AddAccessRule(rule);
                }
            }

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath)));

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                System.Diagnostics.Process.Start("ln", $"-s \"{Path.Combine(path, "deployclient")}\" /bin/deployc");

                var envs = Environment.GetEnvironmentVariable("PATH");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("PATH", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }
            else
            {
                var envs = Environment.GetEnvironmentVariable("Path");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("Path", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }

            AppCommands.Logger.AppendInfo($"Success installed. Can call \"deployclient\" with args from console to execute deploy commands or you can invoke \"deployclient help\" for get available command list");


            if (!quit)
            {
                Console.ReadKey();
                AppCommands.Logger.AppendInfo("Press any key for close...");
            }

            return CommandReadStateEnum.Success;
        }
    }
}
