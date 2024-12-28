using NSL.Deploy.Host.Utils.Commands;
using NSL.Logger;
using NSL.Utils.CommandLine.CLHandles;
using System;
using System.Threading.Tasks;

namespace ServerPublisher.Server
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            PublisherServer.InitializeApp();

            await CLHandler<AppCommands>.Instance
                .ProcessCommand(new NSL.Utils.CommandLine.CommandLineArgsReader(new NSL.Utils.CommandLine.CommandLineArgs()));
        }
    }
}
