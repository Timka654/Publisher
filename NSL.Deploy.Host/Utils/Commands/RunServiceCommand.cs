using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class RunServiceCommand : CLHandler
    {
        public override string Command => "service";

        public override string Description { get => ""; set => base.Description = value; }

        public RunServiceCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            await PublisherServer.RunServer();

            return CommandReadStateEnum.Success;
        }
    }
}
