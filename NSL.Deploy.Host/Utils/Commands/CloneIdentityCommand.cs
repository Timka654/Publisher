using ServerPublisher.Server.Info;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("source_project_id", typeof(string))]
    [CLArgument("only_private", typeof(bool))]
    internal class CloneIdentityCommand : CLHandler
    {
        public override string Command => "clone_identity";

        public override string Description { get => ""; set => base.Description = value; }

        public CloneIdentityCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("source_project_id")] string sourceProjectId { get; set; }

        [CLArgumentValue("only_private")] bool onlyPrivate { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Clone identity");

            ServerProjectInfo? pidest = values.GetProject();

            if (pidest != null)
            {
                ServerProjectInfo pisrc = PublisherServer.ProjectsManager.GetProject(sourceProjectId);

                if (pisrc == null)
                {
                    AppCommands.Logger.AppendError($"project by source_project_id = {sourceProjectId} not found");

                    return CommandReadStateEnum.Failed;
                }

                if (!values.ConfirmCommandAction(AppCommands.Logger))
                    return CommandReadStateEnum.Cancelled;

                var files = new DirectoryInfo(pisrc.UsersDirPath).GetFiles("*.priuk");

                var priKeyCount = files.Length;

                foreach (var item in files)
                {
                    item.CopyTo(Path.Combine(pidest.UsersDirPath, item.Name).GetNormalizedPath(), true);
                }

                if (!onlyPrivate)
                {
                    files = new DirectoryInfo(pisrc.UsersPublicsDirPath).GetFiles("*.pubuk");

                    var pubKeyCount = files.Length;

                    foreach (var item in files)
                    {
                        item.CopyTo(Path.Combine(pidest.UsersPublicsDirPath, item.Name).GetNormalizedPath(), true);
                    }

                    AppCommands.Logger.AppendError($"{priKeyCount} private and {pubKeyCount} public keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");

                    return CommandReadStateEnum.Success;
                }

                AppCommands.Logger.AppendError($"{priKeyCount} private keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");
            }
            return CommandReadStateEnum.Success;
        }
    }
}
