using System.Runtime.InteropServices;
using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    internal class DevClearInvalidPathCommand : CLHandler
    {
        public override string Command => "dev_clear_invalid_path";

        public override string Description { get => ""; set => base.Description = value; }

        public DevClearInvalidPathCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Try DevClearInvalidPath");

            foreach (var item in PublisherServer.ProjectsManager.GetProjects())
            {
                AppCommands.Logger.AppendInfo($"Start process {item.Info.Name}");

                int c = 0;

                foreach (var file in item.FileInfoList)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (file.FileInfo.Name.Contains('/'))
                        {
                            file.FileInfo.Delete();
                            c++;
                        }
                    }
                    else
                    {
                        if (file.FileInfo.Name.Contains('\\'))
                        {
                            file.FileInfo.Delete();
                            c++;
                        }
                    }
                }

                AppCommands.Logger.AppendInfo($"Cleared {c}");

                item.ReIndexing();
            }

            return CommandReadStateEnum.Success;
        }
    }
}
