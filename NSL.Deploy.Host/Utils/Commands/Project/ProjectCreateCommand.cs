using ServerPublisher.Server.Info;
using System;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands.Project
{
    [CLHandleSelect("projects")]
    [CLArgument("directory", typeof(string))]
    [CLArgument("name", typeof(string))]
    [CLArgument("project_id", typeof(string), true)]
    [CLArgument("full_replace", typeof(bool))]
    [CLArgument("backup", typeof(bool))]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class ProjectCreateCommand : CLHandler
    {
        public override string Command => "create";

        public override string Description { get => "Create and link new project on deploy host"; set => base.Description = value; }

        public ProjectCreateCommand()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("projects_create", true));
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("name")] private string name { get; set; }

        [CLArgumentValue("project_id")] private string? projectId { get; set; }

        [CLArgumentValue("full_replace")] private bool fullReplace { get; set; }

        [CLArgumentValue("backup", true)] private bool backup { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create project");

            values.GetWorkingDirectory("directory", out string directory);

            if (PublisherServer.ProjectsManager.ExistProject(directory))
            {
                AppCommands.Logger.AppendError($"Project in folder {directory} already attached");
                return CommandReadStateEnum.Failed;
            }

            if (projectId != default && PublisherServer.ProjectsManager.ExistProjectById(projectId))
            {
                AppCommands.Logger.AppendError($"Project with id {projectId} already exists");
                return CommandReadStateEnum.Failed;
            }

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Cancelled;
            /*
             
                Id = args.ContainsKey("project_id") ? args["project_id"] : Guid.NewGuid().ToString(),
                Name = args["name"],
                FullReplace = args.ContainsKey("full_replace") && Convert.ToBoolean(args["full_replace"]),
                Backup = (args.ContainsKey("backup") && Convert.ToBoolean(args["backup"])) || !args.ContainsKey("backup"),
             */
            var proj = new ServerProjectInfo(projectId, name, fullReplace, backup, directory);

            PublisherServer.ProjectsManager.AddProject(proj);

            PublisherServer.ProjectsManager.SaveProjLibrary();


            AppCommands.Logger.AppendInfo($"project {proj.Info.Name} by id {proj.Info.Id} created");

            return CommandReadStateEnum.Success;
        }
    }
}
