using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    internal class ReindexingCommand : CLHandler
    {
        public override string Command => "reindexing";

        public override string Description { get => ""; set => base.Description = value; }

        public ReindexingCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

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
