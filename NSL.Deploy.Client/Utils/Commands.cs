using Newtonsoft.Json;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Utils;
using ServerPublisher.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ServerPublisher.Client.Utils
{
    public class Commands
    {
        static ILogger Logger = ConsoleLogger.Create();

        private record struct Command(Action<CommandLineArgs> action, string helpContent, string helpDetailsContent);

        private static readonly Dictionary<string, Command> commands = new()
        {
            { "publish", new Command(PublishCommand, "publish project", """
Command for publish project, parameters:
project_id:string - project identity on server
directory:string - absolute or relative path to root project directory with build
auth_key_path:string - absolute or relative path to publish public key file, and check global key storage - "key_storage" folder in publisher directory
cipher_out_key:string - network security encryption key, must equals with input key on server
cipher_in_key:string - network security decryption key, must equals with output key on server
ip:string - server network external ip address
port:int - server network port
buffer_len:int - split data max length for transport
success_args:string? - send args to server for execution OnEnd script
has_compression:bool - mark for compression data to transport
""") },
            { "install", new Command(InstallCommand, "install or update exists app", """
Command for install or update app, configure for integrate app in your os for fast execute, parameters:
path:string? - path for install app, have default value specific for your os
""") },
            { "init", new Command(InitCommand, "check or init folders/configuration for app", "") },
            { "copy_template", new Command(DeployTemplateCommand, "obsolete. see deploy_template", string.Empty) },
            { "deploy_template", new Command(DeployTemplateCommand,"deploy template to current directory", """
Command for copy template to current folder, parameters:
name:string - template name
All templates must contains in folder "templates" in app directory
""") },
            { "install_template", new Command(InstallTemplate, "install or replace template", """
Command for clone current directory template to app templates library
""") },
            { "install_global_keys", new Command(InstallGlobalKeysCommand, "install or replace global key", """
Command for clone all *.pubuk from current directory to app key library
""") }
        };

        private static void PublishCommand(CommandLineArgs cmd) => new Publish()
            .Run(cmd)
            .ConfigureAwait(true)
            .GetAwaiter()
            .GetResult();

        private static void InstallGlobalKeysCommand(CommandLineArgs cmd)
        {
            if (RequireRunningAsAdministrator())
                return;

            var dir = Directory.GetCurrentDirectory();

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatePath = Path.Combine(appPath, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath));

            Logger.AppendInfo($"Move from {dir} to {templatePath}?");

            if (!cmd.ConfirmAction(Logger))
                return;

            IOUtils.CreateDirectoryIfNoExists(templatePath);

            foreach (var item in Directory.GetFiles(dir, "*.pubuk", SearchOption.AllDirectories))
            {
                var epath = Path.Combine(templatePath, Path.GetFileName(item));

                Logger.AppendInfo($"Copy \"{item}\" => \"{epath}\"");

                File.Copy(item, epath, true);
            }
        }

        private static void InstallTemplate(CommandLineArgs cmd)
        {
            if (RequireRunningAsAdministrator())
                return;

            var dir = Directory.GetCurrentDirectory();

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatePath = Path.Combine(appPath, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath), Path.GetDirectoryName(dir));

            IOUtils.CreateDirectoryIfNoExists(templatePath);

            Logger.AppendInfo($"Move from {dir} to {templatePath}?");

            if (!cmd.ConfirmAction(Logger))
                return;

            CopyDirectory(dir, templatePath, true);
        }

        private static void InstallCommand(CommandLineArgs cmd)
        {
            if (RequireRunningAsAdministrator())
                return;

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            var isDefault = cmd.ContainsKey("default");


            if (!cmd.TryGetOutValue("path", out string path))
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    path = "/etc/nsldeployclient";
                }
                else
                {
                    //if (Environment.Is64BitProcess)
                    //    path = @"C:\Program Files (x86)\Publisher.Client";
                    //else
                    path = @"C:\Program Files\NSL.Deploy.Client";
                }

                if (!isDefault)
                    path = CommandParameterReader.Read("Install directory", Logger, path);
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            CopyDirectory(appPath, path, true, true, []);

            InitData(path);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                foreach (var item in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    FileSystemAccessRule rule = new("Everyone", FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                    new FileInfo(item).GetAccessControl().AddAccessRule(rule);
                }
            }

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath)));

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                System.Diagnostics.Process.Start("ln", $"-s \"{Path.Combine(path, "deployclient")}\" /bin/deployc");

                var envs = Environment.GetEnvironmentVariable("PATH");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("PATH", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }
            else
            {
                var envs = Environment.GetEnvironmentVariable("Path");

                if (!envs.Contains(path))
                    Environment.SetEnvironmentVariable("Path", $"{path};{envs}", EnvironmentVariableTarget.Machine);
            }

            if (!cmd.ContainsKey("q"))
                Console.ReadKey();
        }

        private static void InitCommand(CommandLineArgs cmd)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            InitData(appPath);
        }

        private static void InitData(string path)
        {
            var configurationPath = Path.Combine(path, "config.json");

            if (!File.Exists(configurationPath))
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(Program.Configuration));

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath)));

            if (!Directory.Exists(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath))))
                Directory.CreateDirectory(Path.Combine(path, Environment.ExpandEnvironmentVariables(Program.Configuration.KeysPath)));
        }

        private static void DeployTemplateCommand(CommandLineArgs cmd)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            string templatesPath = Path.Combine(appPath, Environment.ExpandEnvironmentVariables(Program.Configuration.TemplatesPath));

            cmd.TryGetOutValue("name", out string name);

            while (string.IsNullOrWhiteSpace(name) || !Directory.Exists(Path.Combine(templatesPath, name).GetNormalizedPath()))
            {
                Console.WriteLine("parameter name is empty or not exists /name:<value>");
                Console.WriteLine("exists values:");

                foreach (var item in Directory.GetDirectories(templatesPath))
                {
                    Console.WriteLine($"- {Path.GetRelativePath(templatesPath, item).GetNormalizedPath()}");
                }

                name = CommandParameterReader.Read<string>($"template name", Logger);
            }

            string templatePath = Path.Combine(templatesPath, name);

            if (Directory.Exists(templatePath))
                foreach (var item in Directory.GetFiles(templatePath))
                {
                    var targetPath = Path.Combine(Directory.GetCurrentDirectory().GetNormalizedPath(), Path.GetRelativePath(templatePath, item).GetNormalizedPath()).GetNormalizedPath();

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

            var actionName = args["action"];

            if (actionName == default)
            {
                if (args.ContainsKey("help"))
                {
                    displayHelp(args, null);
                    return true;
                }

                Console.WriteLine("Commands is empty");
                displayHelp(args, null);
                return false;
            }
            if (!commands.TryGetValue(actionName, out var action))
            {
                Console.WriteLine($"Command not found {actionName}");
                return true;
            }

            if (args.ContainsKey("help"))
            {
                displayHelp(args, actionName);
                return true;
            }


            action.action(args);

            return true;
        }

        private static void displayHelp(CommandLineArgs args, string? action)
        {
            Console.WriteLine();

            if (action == null)
            {
                foreach (var item in commands)
                {
                    Console.WriteLine($"Command \"{item.Key}\" - {item.Value.helpContent}");
                    Console.WriteLine();
                }

                return;
            }

            if (commands.TryGetValue(action, out var cmd))
            {
                Console.WriteLine($"Command \"{action}\"");
                Console.WriteLine(cmd.helpDetailsContent);
                Console.WriteLine();
            }
        }
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool overwrite = true, string[] exclude = null)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);

                if (exclude?.Any(x => targetFilePath.EndsWith(x)) == true)
                    continue;

                Logger.AppendInfo($"Copy from {file.FullName} to {targetFilePath}");
                file.CopyTo(targetFilePath, overwrite);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true, overwrite);
                }
            }
        }



        static bool RequireRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    RestartAsAdministrator(Environment.GetCommandLineArgs());
                    return true;
                }
            }

            return false;
        }

        static void RestartAsAdministrator(string[] args)
        {
            // Get the current process's executable path
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            // Build the arguments string
            string arguments = string.Join(" ", args);

            // Start the process with administrator privileges
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas" // Request administrator privileges
            };

            try
            {
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to restart as administrator: {ex.Message}");
            }
        }
    }
}
