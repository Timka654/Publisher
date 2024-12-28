using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class CheckScriptsCommand : CLHandler
    {
        public override string Command => "check_scripts";

        public override string Description { get => ""; set => base.Description = value; }

        public CheckScriptsCommand()
        {

        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            AppCommands.Logger.AppendInfo("Check Scripts");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;

            var project = values.GetProject();

            if (project == null)
                return CommandReadStateEnum.Failed;

            project.CheckScripts();

            return CommandReadStateEnum.Success;
        }
    }
}
