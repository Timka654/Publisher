using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.Project
{
    [CLHandleSelect("projects")]
    [CLArgument("projectId", typeof(string), optional: true)]
    [CLArgument("directory", typeof(string), optional: true)]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class ProjectReindexingCommand : CLHandler
    {
        public override string Command => "reindexing";

        public override string Description { get => ""; set => base.Description = value; }

        public ProjectReindexingCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Try reindexing");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;

            var project = values.GetProject();

            if (project == null)
                return CommandReadStateEnum.Failed;

            project.ReIndexing();

            return CommandReadStateEnum.Success;
        }
    }
}
