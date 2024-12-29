using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System.IO;

namespace NSL.Deploy.Host.Utils.Commands.Project.Template
{
    [CLHandleSelect("projects_template")]
    [CLArgument("template_path", typeof(string), true)]
    internal class ProjectTemplatePublishCommand : CLHandler
    {
        public override string Command => "publish";

        public override string Description { get => "Create template structure and files for deploy/register project"; set => base.Description = value; }

        public ProjectTemplatePublishCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create project");

            var basePath = Directory.GetCurrentDirectory();

            var endPath = Path.GetFullPath(Path.Combine(basePath, "Publisher", "template.json"));

            var fi = new FileInfo(endPath);

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            if (fi.Exists)
            {
                AppCommands.Logger.AppendError($"Template already exists");
            }
            else
            {
                File.WriteAllText(fi.FullName, """
                    {
                    	"ProjectInfo": {
                            "Name":"example",
                            "FullReplace": false,
                            "Backup": true,
                    		"IgnoreFilePaths": [
                    			"appsettings.[\\s|\\S]*"
                    		],
                        },
                        "Users":[
                    		//{
                    		//	"Name": "uexample"
                    		//}
                        ]
                    }
                    """);

                AppCommands.Logger.AppendInfo($"Success");
            }
            return CommandReadStateEnum.Success;
        }
    }
}
