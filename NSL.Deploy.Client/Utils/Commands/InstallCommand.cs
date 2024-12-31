using NSL.Logger;
using NSL.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
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
#if RELEASE
            if (PermissionUtils.RequireRunningAsAdministrator())
                return CommandReadStateEnum.Success;
#endif

            ProcessingAutoArgs(values);

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            var path = this.path;

            if (!containsPathArg)
            {
                if (isLinux)
                {
                    path = "/etc/nsldeployclient";
                }
                else if (isWindows)
                {
                    //if (Environment.Is64BitProcess)
                    //    path = @"C:\Program Files (x86)\Publisher.Client";
                    //else
                    path = @"C:\Program Files\NSL.Deploy.Client";
                }
                else throw new PlatformNotSupportedException();

                if (!defaultConfiguration)
                    path = CommandParameterReader.Read("Install directory", AppCommands.Logger, path);
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            IOUtils.CopyDirectory(appPath, path, true, filter: (targetFilePath, file) =>
            {
                if (!targetFilePath.EndsWith(".pdb") && !targetFilePath.EndsWith(".exe"))
                {
                    if (File.Exists(targetFilePath))
                    {
                        AppCommands.Logger.AppendError($"'{targetFilePath}' already exists - skip");
                        return false;
                    }
                }

                AppCommands.Logger.AppendInfo($"Copy from {file.FullName} to {targetFilePath}");

                return true;
            });

            Program.InitData();

            if (isLinux)
            {
                System.Diagnostics.Process.Start("ln", $"-s \"{Path.Combine(path, "deployclient")}\" /bin/deployc");

                var envs = Environment.GetEnvironmentVariable("PATH");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("PATH", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }
            else if (isWindows)
            {
                var envs = Environment.GetEnvironmentVariable("Path");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("Path", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }
            else throw new PlatformNotSupportedException();

            AppCommands.Logger.AppendInfo($"Success installed. Can call \"deployclient\" with args from console to execute deploy commands or you can invoke \"deployclient help\" for get available command list");


            if (!quit)
            {
                AppCommands.Logger.AppendInfo("Press any key for close...");
                Console.ReadKey();
            }

            return CommandReadStateEnum.Success;
        }
    }
}
