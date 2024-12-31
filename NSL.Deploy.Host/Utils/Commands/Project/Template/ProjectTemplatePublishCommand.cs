using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System.IO;

namespace NSL.Deploy.Host.Utils.Commands.Project.Template
{
    [CLHandleSelect("projects_template")]
    [CLArgument("root_path", typeof(string), true, Description = "Path to root project directory for produce deploy template content")]
    [CLArgument("n", typeof(bool), true, Description = "Create new directory if no exists, default = false")]
    internal class ProjectTemplatePublishCommand : CLHandler
    {
        public override string Command => "publish";

        public override string Description { get => "Produce template content for deploy/register project"; set => base.Description = value; }

        public ProjectTemplatePublishCommand()
        {
            AddArguments(SelectArguments());
        }
        [CLArgumentExists("root_path")] private bool RootPathExists { get; set; }
        [CLArgumentValue("root_path")] private string RootPath { get; set; }

        [CLArgumentValue("n")] private bool newDir { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            AppCommands.Logger.AppendInfo("Create project");

            var basePath = Directory.GetCurrentDirectory();

            if (RootPathExists)
                basePath = RootPath;

            if (!Directory.Exists(basePath))
            {
                if (newDir)
                {
                    Directory.CreateDirectory(basePath);
                }
                else throw new IOException($"Directory not exists");
            }

            var relativePath = Path.Combine(basePath, "Publisher");

            var endPath = Path.GetFullPath(Path.Combine(relativePath, "template.json"));

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
