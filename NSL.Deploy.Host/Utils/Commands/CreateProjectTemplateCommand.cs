using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("createProjectCommands")]
    [CLArgument("template_path", typeof(string), true)]
    internal class CreateProjectTemplateCommand : CLHandler
    {
        public override string Command => "template";

        public override string Description { get => ""; set => base.Description = value; }

        public CreateProjectTemplateCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create project");

            return await CLHandler<UpdateProjectCommand>.Instance.ProcessCommand(reader, values);
        }
    }
}
