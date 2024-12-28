using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class InitCommand : CLHandler
    {
        public override string Command => "init";

        public override string Description { get => "check and init default folders/configuration for application"; set => base.Description = value; }

        public InitCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            AppCommands.InitData(appPath);

            return CommandReadStateEnum.Success;
        }
    }
}
