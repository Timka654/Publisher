using NSL.Utils.CommandLine.CLHandles;

namespace NSL.Deploy.Host.Utils.Commands.Project
{
    [CLHandleSelect("default")]
    internal class ProjectsCommand : CLHandler
    {
        public override string Command => "projects";

        public override string Description { get => ""; set => base.Description = value; }

        public ProjectsCommand()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("projects", true));
        }
    }
}
