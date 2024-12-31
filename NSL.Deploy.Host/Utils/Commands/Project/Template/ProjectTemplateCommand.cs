using NSL.Utils.CommandLine.CLHandles;

namespace NSL.Deploy.Host.Utils.Commands.Project.Template
{
    [CLHandleSelect("projects")]
    internal class ProjectTemplateCommand : CLHandler
    {
        public override string Command => "template";

        public override string Description { get => "Create and link new project on deploy host with \"Publisher/template.json\" template file"; set => base.Description = value; }

        public ProjectTemplateCommand()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("projects_template", true));
        }
    }
}
