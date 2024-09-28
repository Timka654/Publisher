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
using NSL.Logger;
using ServerPublisher.Shared.Info;
using Microsoft.Extensions.Configuration;
using ServerPublisher.Shared.Utils;
using System.Text.Json;
using Newtonsoft.Json;

namespace ServerPublisher.Server.Utils
{
    public class Commands
    {
        protected static Dictionary<string, Action<CommandLineArgs>> commands;

        static Commands()
        {
            commands = new Dictionary<string, Action<CommandLineArgs>>()
            {
                { "service", RunService },
                { "create_project", CreateProject },
                { "update_project", UpdateProject },
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
                Logger.AppendInfo($"Cannot find paramater {name}. Try set current directory - {value}");
                return false;
            }

            return true;
        }

        private static bool ConfirmAction(CommandLineArgs args)
        {
            if (args.TryGetOutValue("flags", out string flags))
            {
                if (flags.Contains("y", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.AppendInfo($"Flags contains 'y' - confirm action");
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
                    Logger.AppendError($"Value cannot be {latestInput}. Try again or press Ctrl+C for cancel");

            } while (true);
        }

        private static void PidOrDirInfo()
        {
            Logger.AppendError($"Current command must have project_id(has GUID format) or directory parameters for identity project");
            Logger.AppendError($"You can not using identity parameters if executing command from directory contains project");
        }

        private static ServerProjectInfo GetProject(CommandLineArgs args)
        {
            ServerProjectInfo projectInfo;

            if (args.TryGetOutValue("project_id", out string projectId))
            {
                if (!Guid.TryParse(projectId, out var _))
                {
                    Logger.AppendError($"Invalid \"project_id\" parameter format - must have GUID format");
                    PidOrDirInfo();
                    return null;
                }

                projectInfo = PublisherServer.ProjectsManager.GetProject(projectId);

                if (projectInfo == null)
                {
                    Logger.AppendError($"Cannot find project by project_id = \"{projectId}\"");
                    PidOrDirInfo();
                }
            }
            else
            {
                Logger.AppendError($"Cannot find project_id parameter. Try get by directory");

                GetDirParameter(args, "directory", out var directory);

                projectInfo = PublisherServer.ProjectsManager.GetProjectByPath(directory);

                if (projectInfo == null)
                {
                    Logger.AppendError($"Cannot find project in \"{directory}\"");
                    PidOrDirInfo();
                }
            }

            return projectInfo;
        }

        protected static void CheckScripts(CommandLineArgs args)
        {
            Logger.AppendInfo("Check Scripts");


            if (!ConfirmAction(args))
                return;

            GetProject(args)?.CheckScripts();
        }

        protected static void ReIndexing(CommandLineArgs args)
        {
            Logger.AppendInfo("Try reindexing");

            if (!ConfirmAction(args))
                return;

            GetProject(args)?.ReIndexing();
        }

        protected static void CreateProject(CommandLineArgs args)
        {
            Logger.AppendInfo("Create project");

            if (args.ContainsKey("template") || args.ContainsKey("template_path"))
            {
                UpdateProject(args);

                return;
            }

            GetDirParameter(args, "directory", out string directory);

            if (!args.ContainsKey("name"))
            {
                Logger.AppendError($"create project must have \"name\" parameter");
                return;
            }

            if (args.TryGetOutValue("project_id", out string projectId) && !Guid.TryParse(projectId, out var _))
            {
                Logger.AppendError($"create project \"project_id\" parameter must have GUID format");
                return;
            }

            if (PublisherServer.ProjectsManager.ExistProject(directory))
            {
                Logger.AppendError($"project {directory} already appended");
                return;
            }

            if (!ConfirmAction(args))
                return;

            var proj = new ServerProjectInfo(args, directory);

            PublisherServer.ProjectsManager.AddProject(proj);

            PublisherServer.ProjectsManager.SaveProjLibrary();


            Logger.AppendInfo($"project {proj.Info.Name} by id {proj.Info.Id} created");
        }

        protected static void UpdateProject(CommandLineArgs args)
        {
            Logger.AppendInfo("Update project from template");

            var basePath = Directory.GetCurrentDirectory();

            var relPath = Path.Combine(basePath, "Publisher");


            if (!GetDirParameter(args, "template_path", out string path))
                path = Path.Combine(relPath, "template.json");

            if (!File.Exists(path))
            {
                Logger.AppendError($"Project template \"{path}\" does not exists!!");
                
                return;
            }


            var template = JsonConvert.DeserializeObject<CreateProjectInfo>(File.ReadAllText(path));


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

            foreach (var item in template.Users)
            {
                var user = UserInfo.CreateUser(item.Name);

                if (projectInfo.AddUser(user))
                    Logger.AppendInfo($"Success append new user {user.Name}");
            }


        }

        protected static void LinkProject(CommandLineArgs args)
        {
            Logger.AppendInfo("Link project");

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
                    Logger.AppendInfo($"Already exist: {exists.ProjectDirPath}");

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

        protected static void CreateUser(CommandLineArgs args)
        {
            Logger.AppendInfo("Create user");

            if (!args.ContainsKey("name"))
            {
                Logger.AppendError($"create user must have \"name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                if (!ConfirmAction(args))
                    return;

                var user = UserInfo.CreateUser(args);

                if (projectInfo.AddUser(user))
                    Logger.AppendInfo($"user {user.Name} by id {user.Id} created");
                else
                {
                    PublisherServer.ServerLogger.AppendError($"{user.Name} already exist in project {user.Name}");
                }
            }
        }

        protected static void AddUser(CommandLineArgs args)
        {
            Logger.AppendInfo("Add user");

            if (!args.TryGetOutValue("path", out string path))
            {
                Logger.AppendError($"Add user must have \"path\" parameter");
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
                    Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} not exists");

                    return;
                }
                if (fileInfo.Extension != "priuk")
                {
                    Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} must have .priuk extension");

                    return;
                }

                var dest = Path.Combine(projectInfo.UsersDirPath, fileInfo.Name);

                File.Copy(path, dest, true);

                Logger.AppendError($"{fileInfo.GetNormalizedFilePath()} private key copied to {projectInfo.Info.Name} project ({dest})");
            }
        }

        protected static void AddPatchConnection(CommandLineArgs args)
        {
            Logger.AppendInfo("Add Patch Connection");

            if (!args.TryGetOutValue("ip_address", out string ip_address))
            {
                Logger.AppendError($"Add Patch Connection must have \"ip_address\" parameter");
                return;
            }

            if (!args.TryGetOutValue("port", out ushort port))
            {
                Logger.AppendError($"Add Patch Connection must have \"port\" parameter");
                return;
            }

            if (!args.TryGetOutValue("input_cipher_key", out string input_cipher_key))
            {
                input_cipher_key = PublisherServer.Configuration.Publisher.Server.Cipher.OutputKey;

                Logger.AppendInfo($"Not contains \"input_cipher_key\" parameter. Set from configuration {input_cipher_key}");
            }

            if (!args.TryGetOutValue("output_cipher_key", out string output_cipher_key))
            {
                output_cipher_key = PublisherServer.Configuration.Publisher.Server.Cipher.InputKey;

                Logger.AppendInfo($"Not contains \"output_cipher_key\" parameter. Set from configuration {output_cipher_key}");
            }

            if (!args.TryGetOutValue("identity_name", out string identity_name))
            {
                Logger.AppendError($"Add Patch Connection must have \"identity_name\" parameter");
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

                Logger.AppendInfo($"Patch connection info changed in {projectInfo.Info.Name}({projectInfo.Info.Id}) project");
            }
        }

        protected static void CloneIdentity(CommandLineArgs args)
        {
            Logger.AppendInfo("Clone identity");

            if (!args.TryGetOutValue("source_project_id", out string sourceProjectId))
            {
                Logger.AppendError($"Clone identity must have \"source_project_id\" parameter");
                return;
            }

            ServerProjectInfo pidest = GetProject(args);

            if (pidest != null)
            {
                ServerProjectInfo pisrc = PublisherServer.ProjectsManager.GetProject(sourceProjectId);

                if (pisrc == null)
                {
                    Logger.AppendError($"project by source_project_id = {sourceProjectId} not found");

                    return;
                }

                if (!ConfirmAction(args))
                    return;

                var files = new DirectoryInfo(pisrc.UsersDirPath).GetFiles("*.priuk");

                var priKeyCount = files.Length;

                foreach (var item in files)
                {
                    item.CopyTo(Path.Combine(pidest.UsersDirPath, item.Name).GetNormalizedPath(), true);
                }

                if (!args.ContainsKey("only_private"))
                {
                    files = new DirectoryInfo(pisrc.UsersPublicksDirPath).GetFiles("*.pubuk");

                    var pubKeyCount = files.Length;

                    foreach (var item in files)
                    {
                        item.CopyTo(Path.Combine(pidest.UsersPublicksDirPath, item.Name).GetNormalizedPath(), true);
                    }

                    Logger.AppendError($"{priKeyCount} private and {pubKeyCount} public keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");

                    return;
                }

                Logger.AppendError($"{priKeyCount} private keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");
            }
        }

        protected static void DevClearInvalidPath(CommandLineArgs args)
        {
            Logger.AppendInfo("Try DevClearInvalidPath");

            foreach (var item in PublisherServer.ProjectsManager.GetProjects())
            {
                Logger.AppendInfo($"Start process {item.Info.Name}");
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

                Logger.AppendInfo($"Cleared {c}");

                item.ReIndexing();
            }
        }

        static FileLogger Logger => PublisherServer.ServerLogger;

        public static bool Process()
        {
            CommandLineArgs args = new CommandLineArgs();


            Logger.AppendInfo($"Read command. Args:");

            foreach (var item in args.GetArgs())
            {
                if (item.Value == default)
                    Logger.AppendInfo($"- {item.Key}");
                else
                    Logger.AppendInfo($"- {item.Key} = {item.Value}");
            }

            var actionName = args["action"];

            if (actionName == default)
            {
                Logger.AppendInfo("Commands is empty");
                return false;
            }

            Logger.AppendInfo($"Process action \"{actionName}\"");

            if (!commands.TryGetValue(actionName, out var action))
            {
                Logger.AppendInfo($"Command not found {actionName}");
                return true;
            }

            PublisherServer.CommandExecutor = !actionName.Equals("service");

            ServerOptions<PublisherNetworkClient> options = new ServerOptions<PublisherNetworkClient>();

            options.HelperLogger = Logger;

            options.LoadManagers<PublisherNetworkClient>(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));

            action(args);

            return true;
        }
    }
}
