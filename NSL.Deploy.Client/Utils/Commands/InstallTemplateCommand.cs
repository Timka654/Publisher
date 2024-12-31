using NSL.Logger;
using NSL.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Utils;
using System.IO;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class InstallTemplateCommand : CLHandler
    {
        public override string Command => "install_template";

        public override string Description { get => "Command for clone current directory template to app templates library"; set => base.Description = value; }

        public InstallTemplateCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            if (PermissionUtils.RequireRunningAsAdministrator())
                return CommandReadStateEnum.Success;

            ProcessingAutoArgs(values);

            var dir = Directory.GetCurrentDirectory();

            string templatePath = Path.Combine(Program.TemplatesPath, new DirectoryInfo(dir).Name);

            IOUtils.CreateDirectoryIfNoExists(templatePath);

            AppCommands.Logger.AppendInfo($"Move from {dir} to {templatePath}?");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Success;

            IOUtils.CopyDirectory(dir, templatePath, true, filter: (targetFilePath, file) =>
            {

                AppCommands.Logger.AppendInfo($"Copy from {file.FullName} to {targetFilePath}");

                return true;
            });

            return CommandReadStateEnum.Success;
        }
    }
}
