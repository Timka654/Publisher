using ServerPublisher.Server.Info;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("path", typeof(string))]
    internal class AddUserCommand : CLHandler
    {
        public override string Command => "add_user";

        public override string Description { get => ""; set => base.Description = value; }

        public AddUserCommand()
        {

        }

        [CLArgumentValue("path")] private string path { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Add user");

            ServerProjectInfo projectInfo = values.GetProject();

            if (projectInfo != null)
            {
                if (!values.ConfirmCommandAction(AppCommands.Logger))
                    return CommandReadStateEnum.Failed;

                var fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    AppCommands.Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} not exists");

                    return CommandReadStateEnum.Failed;
                }
                if (fileInfo.Extension != "priuk")
                {
                    AppCommands.Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} must have .priuk extension");

                    return CommandReadStateEnum.Failed;
                }

                var dest = Path.Combine(projectInfo.UsersDirPath, fileInfo.Name);

                File.Copy(path, dest, true);

                AppCommands.Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} private key copied to {projectInfo.Info.Name} project ({dest})");
            }

            return CommandReadStateEnum.Success;
        }
    }
}
