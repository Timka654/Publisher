using NSL.Logger;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using ServerPublisher.Client;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NSL.Deploy.Client.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("name", typeof(string), true, Description = "Name of exists template")]
    internal class DeployTemplateCommand : CLHandler
    {
        public override string Command => "deploy_template";

        public override string Description { get => "Command for copy template to current folder"; set => base.Description = value; }

        public DeployTemplateCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentValue("name")] public string Name { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            ProcessingAutoArgs(values);

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatesPath = Path.Combine(appPath, Program.TemplatesPath);

            var name = Name;

            while (string.IsNullOrWhiteSpace(name) || !Directory.Exists(Path.Combine(templatesPath, name).GetNormalizedPath()))
            {
                AppCommands.Logger.AppendInfo("parameter name is empty or not exists /name:<value>");
                AppCommands.Logger.AppendInfo("exists values:");

                foreach (var item in Directory.GetDirectories(templatesPath))
                {
                    AppCommands.Logger.AppendInfo($"- {Path.GetRelativePath(templatesPath, item).GetNormalizedPath()}");
                }

                name = CommandParameterReader.Read<string>($"template name", AppCommands.Logger);
            }

            string templatePath = Path.Combine(templatesPath, name);

            if (Directory.Exists(templatePath))
                foreach (var item in Directory.GetFiles(templatePath))
                {
                    var targetPath = Path.Combine(Directory.GetCurrentDirectory().GetNormalizedPath(), Path.GetRelativePath(templatePath, item).GetNormalizedPath()).GetNormalizedPath();

                    try
                    {
                        AppCommands.Logger.AppendInfo($"Copy \"{item}\" to \"{targetPath}\"");
                        File.Copy(item, targetPath);
                    }
                    catch (Exception ex)
                    {
                        AppCommands.Logger.AppendInfo($"File already exists or cannot access to target path - {ex}");
                    }

                    AppCommands.Logger.AppendInfo("Finished!!");
                }

            return CommandReadStateEnum.Success;
        }
    }
}
