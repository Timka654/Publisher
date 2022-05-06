using ServerPublisher.Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerPublisher.Client.Utils
{
    public class Commands
    {
        private static readonly Dictionary<string, Action<CommandLineArgs>> commands = new Dictionary<string, Action<CommandLineArgs>>()
        {
            { "publish", PublishCommand },
            { "copy_template", CopyTemplateCommand }
        };

        private static void PublishCommand(CommandLineArgs cmd) => new Publish()
            .Run(cmd)
            .ConfigureAwait(true)
            .GetAwaiter()
            .GetResult();

        private static void CopyTemplateCommand(CommandLineArgs cmd)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatesPath = Path.Combine(appPath, "Templates");

            string name = default;

            if (!cmd.TryGetValue("name", ref name) || string.IsNullOrWhiteSpace(name) || !Directory.Exists(Path.Combine(templatesPath, name)))
            {
                Console.WriteLine("parameter name is empty or not exists /name:<value>");
                Console.WriteLine("exists values:");

                foreach (var item in Directory.GetDirectories(templatesPath))
                {
                    Console.WriteLine($"- {Path.GetRelativePath(templatesPath, item)}");
                }
                return;
            }

            string templatePath = Path.Combine(templatesPath, name);


            foreach (var item in Directory.GetFiles(templatePath))
            {
                var targetPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRelativePath(templatePath, item));

                try
                {
                    Console.WriteLine($"Copy \"{item}\" to \"{targetPath}\"");
                    File.Copy(item, targetPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File already exists or cannot access to target path - {ex}");
                }

                Console.WriteLine("Finished!!");
            }
        }

        public static bool Process()
        {
            CommandLineArgs args = new CommandLineArgs();

            if (args["action"] == default)
            {
                Console.WriteLine("Commands is empty");
                return false;
            }
            if (!commands.TryGetValue(args["action"], out var action))
            {
                Console.WriteLine($"Command not found {args["action"]}");
                return true;
            }

            action(args);

            return true;
        }
    }
}
