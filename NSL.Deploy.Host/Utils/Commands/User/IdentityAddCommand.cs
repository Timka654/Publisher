using ServerPublisher.Server.Info;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.User
{
    [CLHandleSelect("identity")]
    [CLArgument("path", typeof(string))]
    [CLArgument("project_id", typeof(string), optional: true)]
    [CLArgument("directory", typeof(string), optional: true)]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class IdentityAddCommand : CLHandler
    {
        public override string Command => "add";

        public override string Description { get => ""; set => base.Description = value; }

        public IdentityAddCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("path")] private string path { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Add exists identity");

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
