using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class PublishCommand : CLHandler
    {
        public override string Command => "publish";

        public override string Description { get => "Publish project to server"; set => base.Description = value; }

        public PublishCommand()
        {

        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            await new Publish()
            .Run(reader.Args);

            return CommandReadStateEnum.Success;
        }
    }
}
