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

namespace NSL.Deploy.Host.Utils.Commands.Project.Template
{
    [CLHandleSelect("projects_template")]
    [CLArgument("root_path", typeof(string), true)]
    internal class ProjectTemplateDeployCommand : CLHandler
    {
        public override string Command => "deploy";

        public override string Description { get => "Update/Create and link project on deploy host with \"Publisher/template.json\" template file"; set => base.Description = value; }

        public ProjectTemplateDeployCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentExists("root_path")] private bool RootPathExists { get; set; }
        [CLArgumentValue("root_path")] private string RootPath { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Update project from template");

            var basePath = Directory.GetCurrentDirectory();

            if (RootPathExists)
                basePath = RootPath;

            basePath = Path.GetFullPath(basePath);

            var relPath = Path.Combine(basePath, "Publisher");

            var templatePath = Path.Combine(relPath, "template.json");


            if (!File.Exists(templatePath))
            {
                AppCommands.Logger.AppendError($"Project template \"{templatePath}\" does not exists!!");

                return CommandReadStateEnum.Failed;
            }


            var template = JsonConvert.DeserializeObject<CreateProjectInfo>(File.ReadAllText(templatePath));


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
