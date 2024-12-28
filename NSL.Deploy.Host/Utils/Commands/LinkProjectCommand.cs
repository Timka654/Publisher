using ServerPublisher.Server.Info;
using System;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("directory", typeof(string))]
    internal class LinkProjectCommand : CLHandler
    {
        public override string Command => "link_project";

        public override string Description { get => ""; set => base.Description = value; }

        public LinkProjectCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Link project");

            values.GetWorkingDirectory("directory", out string directory);

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;

            try
            {
                var proj = new ServerProjectInfo(directory);

                var exists = PublisherServer.ProjectsManager.GetProject(proj);

                if (exists != null && proj.ProjectDirPath == exists.ProjectDirPath)
                    return CommandReadStateEnum.Success;

                else if (exists != null)
                {
                    AppCommands.Logger.AppendInfo($"Already exist: {exists.ProjectDirPath}");

                    if (!values.ConfirmCommandAction(AppCommands.Logger))
                        return CommandReadStateEnum.Cancelled;

                    PublisherServer.ProjectsManager.RemoveProject(exists, false);
                }
                PublisherServer.ProjectsManager.AddProject(proj);

                PublisherServer.ProjectsManager.SaveProjLibrary();
            }
            catch (Exception)
            {

                throw;
            }

            return CommandReadStateEnum.Success;
        }
    }
}
