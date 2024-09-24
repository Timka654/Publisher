using NSL.Utils;
using NSL.ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Network.PublisherClient;
using NSL.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NSL.Logger;
using ServerPublisher.Shared.Info;
using Microsoft.Extensions.Configuration;

namespace ServerPublisher.Server.Utils
{
    public class Commands
    {
        protected readonly Dictionary<string, Action<CommandLineArgs>> commands;

        public Commands()
        {
            commands = new Dictionary<string, Action<CommandLineArgs>>()
            {
                { "service", RunService },
                { "create_project", CreateProject },
                { "link_project", LinkProject },
                { "create_user", CreateUser },
                { "add_user", AddUser },
                { "add_patch_connection", AddPatchConnection },
                { "clone_identity", CloneIdentity },
                { "check_scripts", CheckScripts },
                { "reindexing", ReIndexing },
                { "dev_clear_invalid_path", DevClearInvalidPath }
            };
        }

        private static void RunService(CommandLineArgs args)
        {
            PublisherServer.RunServer();
        }

        private static bool GetDirParameter(CommandLineArgs args, string name, out string value)
        {
            value = default;

            if (!args.TryGetValue(name, ref value))
            {
                value = Directory.GetCurrentDirectory();
                PublisherServer.ServerLogger.AppendInfo($"Cannot find paramater {name}. Try set current directory - {value}");
                return false;
            }

            return true;
        }

        private bool ConfirmAction(CommandLineArgs args)
        {
            if (args.TryGetOutValue("flags", out string flags))
            {
                if (flags.Contains("y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Flags contains 'y' - confirm action");
                    return true;
                }
            }

            string latestInput = default;

            do
            {
                Console.Write("You confirm action? 'y' - yes/'n' - no:");

                latestInput = Console.ReadLine();

                if (latestInput.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (latestInput.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return false;
                else
                    Console.WriteLine($"Value cannot be {latestInput}. Try again or press Ctrl+C for cancel");

            } while (true);
        }

        private static void PidOrDirInfo()
        {
            PublisherServer.ServerLogger.AppendError($"Current command must have project_id(has GUID format) or directory parameters for identity project");
            PublisherServer.ServerLogger.AppendError($"You can not using identity parameters if executing command from directory contains project");
        }

        private static ServerProjectInfo GetProject(CommandLineArgs args)
        {
            ServerProjectInfo projectInfo;

            if (args.TryGetOutValue("project_id", out string projectId))
            {
                if (!Guid.TryParse(projectId, out var _))
                {
                    PublisherServer.ServerLogger.AppendError($"Invalid \"project_id\" parameter format - must have GUID format");
                    PidOrDirInfo();
                    return null;
                }

                projectInfo = PublisherServer.ProjectsManager.GetProject(projectId);

                if (projectInfo == null)
                {
                    PublisherServer.ServerLogger.AppendError($"Cannot find project by project_id = \"{projectId}\"");
                    PidOrDirInfo();
                }
            }
            else
            {
                PublisherServer.ServerLogger.AppendError($"Cannot find project_id parameter. Try get by directory");

                GetDirParameter(args, "directory", out var directory);

                projectInfo = PublisherServer.ProjectsManager.GetProjectByPath(directory);

                if (projectInfo == null)
                {
                    PublisherServer.ServerLogger.AppendError($"Cannot find project in \"{directory}\"");
                    PidOrDirInfo();
                }
            }

            return projectInfo;
        }

        protected void CheckScripts(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Check Scripts");


            if (!ConfirmAction(args))
                return;

            GetProject(args)?.CheckScripts();
        }

        protected void ReIndexing(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Try reindexing");

            if (!ConfirmAction(args))
                return;

            GetProject(args)?.ReIndexing();
        }

        protected void CreateProject(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Create project");

            GetDirParameter(args, "directory", out string directory);

            if (!args.ContainsKey("name"))
            {
                PublisherServer.ServerLogger.AppendError($"create project must have \"name\" parameter");
                return;
            }

            if (args.TryGetOutValue("project_id", out string projectId) && !Guid.TryParse(projectId, out var _))
            {
                PublisherServer.ServerLogger.AppendError($"create project \"project_id\" parameter must have GUID format");
                return;
            }

            if (PublisherServer.ProjectsManager.ExistProject(directory))
            {
                PublisherServer.ServerLogger.AppendError($"project {directory} already appended");
                return;
            }

            if (!ConfirmAction(args))
                return;

            var proj = new ServerProjectInfo(args, directory);

            PublisherServer.ProjectsManager.AddProject(proj);

            PublisherServer.ProjectsManager.SaveProjLibrary();


            PublisherServer.ServerLogger.AppendInfo($"project {proj.Info.Name} by id {proj.Info.Id} created");
        }

        protected void LinkProject(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Link project");

            GetDirParameter(args, "directory", out string directory);

            if (!ConfirmAction(args))
                return;

            try
            {
                var proj = new ServerProjectInfo(directory);

                var exists = PublisherServer.ProjectsManager.GetProject(proj);

                if (exists != null && proj.ProjectDirPath == exists.ProjectDirPath)
                    return;
                else if (exists != null)
                {
                    Console.WriteLine($"Already exist: {exists.ProjectDirPath}");

                    if (!ConfirmAction(args))
                        return;

                    PublisherServer.ProjectsManager.RemoveProject(exists, false);
                }
                PublisherServer.ProjectsManager.AddProject(proj);

                PublisherServer.ProjectsManager.SaveProjLibrary();
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void CreateUser(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Create user");

            if (!args.ContainsKey("name"))
            {
                PublisherServer.ServerLogger.AppendError($"create user must have \"name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                if (!ConfirmAction(args))
                    return;

                var user = new UserInfo(args);

                if (projectInfo.AddUser(user))
                    PublisherServer.ServerLogger.AppendInfo($"user {user.Name} by id {user.Id} created");
                else
                    PublisherServer.ServerLogger.AppendError($"user {user.Name} already exists");
            }
        }

        protected void AddUser(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Add user");

            if (!args.TryGetOutValue("path", out string path))
            {
                PublisherServer.ServerLogger.AppendError($"Add user must have \"path\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {

                if (!ConfirmAction(args))
                    return;

                var fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    PublisherServer.ServerLogger.AppendError($"{fileInfo.FullName} not exists");

                    return;
                }
                if (fileInfo.Extension != "priuk")
                {
                    PublisherServer.ServerLogger.AppendError($"{fileInfo.FullName} must have .priuk extension");

                    return;
                }

                var dest = Path.Combine(projectInfo.UsersDirPath, fileInfo.Name);

                File.Copy(path, dest, true);

                PublisherServer.ServerLogger.AppendError($"{fileInfo.FullName} private key copied to {projectInfo.Info.Name} project ({dest})");
            }
        }

        protected void AddPatchConnection(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Add Patch Connection");

            if (!args.TryGetOutValue("ip_address", out string ip_address))
            {
                PublisherServer.ServerLogger.AppendError($"Add Patch Connection must have \"ip_address\" parameter");
                return;
            }

            if (!args.TryGetOutValue("port", out ushort port))
            {
                PublisherServer.ServerLogger.AppendError($"Add Patch Connection must have \"port\" parameter");
                return;
            }

            if (!args.TryGetOutValue("input_cipher_key", out string input_cipher_key))
            {
                input_cipher_key = PublisherServer.Configuration.GetValue<string>("server.io.output.key");

                PublisherServer.ServerLogger.AppendInfo($"Not contains \"input_cipher_key\" parameter. Set from configuration {input_cipher_key}");
            }

            if (!args.TryGetOutValue("output_cipher_key", out string output_cipher_key))
            {
                output_cipher_key = PublisherServer.Configuration.GetValue<string>("server.io.input.key");

                PublisherServer.ServerLogger.AppendInfo($"Not contains \"output_cipher_key\" parameter. Set from configuration {output_cipher_key}");
            }

            if (!args.TryGetOutValue("identity_name", out string identity_name))
            {
                PublisherServer.ServerLogger.AppendError($"Add Patch Connection must have \"identity_name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                if (!ConfirmAction(args))
                    return;

                projectInfo.UpdatePatchInfo(new ProjectPatchInfo()
                {
                    IpAddress = ip_address,
                    Port = (int)port,
                    InputCipherKey = input_cipher_key,
                    OutputCipherKey = output_cipher_key,
                    SignName = identity_name
                });

                PublisherServer.ServerLogger.AppendInfo($"Patch connection info changed in {projectInfo.Info.Name}({projectInfo.Info.Id}) project");
            }
        }

        protected void CloneIdentity(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Clone identity");

            if (!args.TryGetOutValue("source_project_id", out string sourceProjectId))
            {
                PublisherServer.ServerLogger.AppendError($"Clone identity must have \"source_project_id\" parameter");
                return;
            }

            ServerProjectInfo pidest = GetProject(args);

            if (pidest != null)
            {
                ServerProjectInfo pisrc = PublisherServer.ProjectsManager.GetProject(sourceProjectId);

                if (pisrc == null)
                {
                    PublisherServer.ServerLogger.AppendError($"project by source_project_id = {sourceProjectId} not found");

                    return;
                }

                if (!ConfirmAction(args))
                    return;

                var files = new DirectoryInfo(pisrc.UsersDirPath).GetFiles("*.priuk");

                var priKeyCount = files.Length;

                foreach (var item in files)
                {
                    item.CopyTo(Path.Combine(pidest.UsersDirPath, item.Name), true);
                }

                if (!args.ContainsKey("only_private"))
                {
                    files = new DirectoryInfo(pisrc.UsersPublicksDirPath).GetFiles("*.pubuk");

                    var pubKeyCount = files.Length;

                    foreach (var item in files)
                    {
                        item.CopyTo(Path.Combine(pidest.UsersPublicksDirPath, item.Name), true);
                    }

                    PublisherServer.ServerLogger.AppendError($"{priKeyCount} private and {pubKeyCount} public keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");

                    return;
                }

                PublisherServer.ServerLogger.AppendError($"{priKeyCount} private keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");
            }
        }

        protected void DevClearInvalidPath(CommandLineArgs args)
        {
            PublisherServer.ServerLogger.AppendInfo("Try DevClearInvalidPath");

            foreach (var item in PublisherServer.ProjectsManager.GetProjects())
            {
                PublisherServer.ServerLogger.AppendInfo($"Start process {item.Info.Name}");
                int c = 0;
                foreach (var file in item.FileInfoList)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (file.FileInfo.Name.Contains('/'))
                        {
                            file.FileInfo.Delete();
                            c++;
                        }
                    }
                    else
                    {
                        if (file.FileInfo.Name.Contains('\\'))
                        {
                            file.FileInfo.Delete();
                            c++;
                        }
                    }
                }

                PublisherServer.ServerLogger.AppendInfo($"Cleared {c}");

                item.ReIndexing();
            }
        }

        public bool Process()
        {
            CommandLineArgs args = new CommandLineArgs();

            if (args["action"] == default)
            {
                PublisherServer.ServerLogger.AppendInfo("Commands is empty");
                return false;
            }
            if (!commands.TryGetValue(args["action"], out var action))
            {
                PublisherServer.ServerLogger.AppendInfo($"Command not found {args["action"]}");
                return true;
            }

            PublisherServer.CommandExecutor = !args["action"].Equals("service");

            ServerOptions<PublisherNetworkClient> options = new ServerOptions<PublisherNetworkClient>();

            options.HelperLogger = PublisherServer.ServerLogger;

            options.LoadManagers<PublisherNetworkClient>(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));

            action(args);

            return true;
        }
    }
}
