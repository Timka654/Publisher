using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Server;
using ServerPublisher.Server.Managers;
using System;
using System.Threading.Tasks;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class DisplayAppInfoCommand : CLHandler
    {
        public override string Command => "info";

        public override string Description { get => "Display application information"; set => base.Description = value; }

        public DisplayAppInfoCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            Console.WriteLine($"Deploy Host");
            Console.WriteLine($"Version: {Program.Version}");
            Console.WriteLine();
            Console.WriteLine($"Application directory path: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Projects data file path: {ProjectsManager.ProjectsFilePath}");
            Console.WriteLine($"Shared users(full) directory path: {ProjectsManager.GlobalBothUsersFolderPath}");
            Console.WriteLine($"Shared users(publish) directory path: {ProjectsManager.GlobalPublishUsersFolderPath}");
            Console.WriteLine($"Shared users(proxy) directory path: {ProjectsManager.GlobalProxyUsersFolderPath}");


            return CommandReadStateEnum.Success;
        }
    }
}
