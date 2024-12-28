using NSL.Logger;
using NSL.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class InstallGlobalKeysCommand : CLHandler
    {
        public override string Command => "install_global_keys";

        public override string Description { get => "Command for clone all *.pubuk from current directory to app key library"; set => base.Description = value; }

        public InstallGlobalKeysCommand()
        {

        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            if (PermissionUtils.RequireRunningAsAdministrator())
                return CommandReadStateEnum.Success;

            var dir = Directory.GetCurrentDirectory();

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatePath = Path.Combine(appPath, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath));

            AppCommands.Logger.AppendInfo($"Move from {dir} to {templatePath}?");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Success;

            IOUtils.CreateDirectoryIfNoExists(templatePath);

            foreach (var item in Directory.GetFiles(dir, "*.pubuk", SearchOption.AllDirectories))
            {
                var epath = Path.Combine(templatePath, Path.GetFileName(item));

                AppCommands.Logger.AppendInfo($"Copy \"{item}\" => \"{epath}\"");

                File.Copy(item, epath, true);
            }

            return CommandReadStateEnum.Success;
        }
    }
}
