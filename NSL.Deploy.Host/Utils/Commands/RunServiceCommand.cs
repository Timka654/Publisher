using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System;
using ServerPublisher.Server.Managers;

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

            PublisherServer.ServiceInvokable = true;

            ClearManager.Initialize();
            ProjectsManager.Initialize();

            try
            {
                await PublisherServer.RunServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            return CommandReadStateEnum.Success;
        }
    }
}
