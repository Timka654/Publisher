using ServerPublisher.Server.Info;
using System;
using System.IO;
using NSL.Logger;
using Newtonsoft.Json;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("template_path", typeof(string))]
    internal class UpdateProjectCommand : CLHandler
    {
        public override string Command => "update_project";

        public override string Description { get => ""; set => base.Description = value; }

        public UpdateProjectCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Update project from template");

            var basePath = Directory.GetCurrentDirectory();

            var relPath = Path.Combine(basePath, "Publisher");

            if (!values.GetWorkingDirectory("template_path", out string path))
                path = Path.Combine(relPath, "template.json");

            if (!File.Exists(path))
            {
                AppCommands.Logger.AppendError($"Project template \"{path}\" does not exists!!");

                return CommandReadStateEnum.Failed;
            }


            var template = JsonConvert.DeserializeObject<CreateProjectInfo>(File.ReadAllText(path));


            string? projectId = default;

            var projectInfoPath = Path.Combine(relPath, "project.json");

            if (File.Exists(projectInfoPath))
            {
                var pi = JsonConvert.DeserializeObject<ProjectInfoData>(File.ReadAllText(projectInfoPath));

                projectId = pi?.Id;
            }
            else
            {
                projectId = template.ProjectInfo?.Id;
            }

            ServerProjectInfo? projectInfo;

            if (projectId == null || (projectInfo = PublisherServer.ProjectsManager.GetProject(projectId)) == null)
            {
                template.ProjectInfo.Id ??= Guid.NewGuid().ToString();

                projectInfo = new ServerProjectInfo(template.ProjectInfo, basePath);

                PublisherServer.ProjectsManager.AddProject(projectInfo);

                PublisherServer.ProjectsManager.SaveProjLibrary();
            }
            else
            {
                template.ProjectInfo.Id = projectInfo.Info.Id;

                template.ProjectInfo.FillUpdatableTo(projectInfo.Info);

                projectInfo.UpdatePatchInfo(template.ProjectInfo.PatchInfo);
            }

            if (template.Users != null)
                foreach (var item in template.Users)
                {
                    var user = UserInfo.CreateUser(item.Name);

                    if (projectInfo.AddUser(user))
                        AppCommands.Logger.AppendInfo($"Success append new user {user.Name}");
                }

            return CommandReadStateEnum.Success;
        }
    }
}
