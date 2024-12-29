using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using System;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
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

            Console.WriteLine($"Deploy Client");
            Console.WriteLine($"Version: {Program.Version}");
            Console.WriteLine();
            Console.WriteLine($"Application directory path: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Application data path: {Program.AppDataFolder}");
            Console.WriteLine($"Key storage path: {Program.KeysPath}");
            Console.WriteLine($"Template storage path: {Program.TemplatesPath}");


            return CommandReadStateEnum.Success;
        }
    }
}
