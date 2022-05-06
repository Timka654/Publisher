using ServerOptions.Extensions.Manager;
using ServerPublisher.Server.Info;
using ServerPublisher.Server.Network.PublisherClient;
using ServerPublisher.Shared;
using SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ServerPublisher.Server.Utils
{
    public class Commands
    {
        protected readonly Dictionary<string, Action<CommandLineArgs>> commands;

        public Commands()
        {
            commands = new Dictionary<string, Action<CommandLineArgs>>()
            {
                { "create_project", CreateProject },
                { "create_user", CreateUser },
                { "add_user", AddUser },
                { "add_patch_connection", AddPatchConnection },
                { "clone_identity", CloneIdentity },
                { "check_scripts", CheckScripts },
                { "reindexing", ReIndexing },
                { "dev_clear_invalid_path", DevClearInvalidPath }
            };
        }

        private static bool GetDirParameter(CommandLineArgs args, string name, out string value)
        {
            value = default;

            if (!args.TryGetValue(name, ref value))
            {
                value = Directory.GetCurrentDirectory();
                StaticInstances.ServerLogger.AppendInfo($"Cannot find paramater {name}. Try set current directory - {value}");
                return false;
            }

            return true;
        }

        private static void PidOrDirInfo()
        {
            StaticInstances.ServerLogger.AppendError($"Current command must have project_id(has GUID format) or directory parameters for identity project");
            StaticInstances.ServerLogger.AppendError($"You can not using identity parameters if executing command from directory contains project");
        }

        private static ServerProjectInfo GetProject(CommandLineArgs args)
        {
            ServerProjectInfo projectInfo;

            if (args.TryGetOutValue("project_id", out string projectId))
            {
                if (!Guid.TryParse(projectId, out var _))
                {
                    StaticInstances.ServerLogger.AppendError($"Invalid \"project_id\" parameter format - must have GUID format");
                    PidOrDirInfo();
                    return null;
                }

                projectInfo = StaticInstances.ProjectsManager.GetProject(projectId);

                if (projectInfo == null)
                {
                    StaticInstances.ServerLogger.AppendError($"Cannot find project by project_id = \"{projectId}\"");
                    PidOrDirInfo();
                }
            }
            else
            {
                StaticInstances.ServerLogger.AppendError($"Cannot find project_id parameter. Try get by directory");

                GetDirParameter(args, "directory", out var directory);

                projectInfo = StaticInstances.ProjectsManager.GetProjectByPath(directory);

                if (projectInfo == null)
                {
                    StaticInstances.ServerLogger.AppendError($"Cannot find project in \"{directory}\"");
                    PidOrDirInfo();
                }
            }

            return projectInfo;
        }

        protected void CheckScripts(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Check Scripts");

            GetProject(args)?.CheckScripts();
        }

        protected void ReIndexing(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Try reindexing");

            GetProject(args)?.ReIndexing();
        }

        protected void CreateProject(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Create project");

            GetDirParameter(args, "directory", out string directory);

            if (!args.ContainsKey("name"))
            {
                StaticInstances.ServerLogger.AppendError($"create project must have \"name\" parameter");
                return;
            }

            if (args.TryGetOutValue("project_id", out string projectId) && !Guid.TryParse(projectId, out var _))
            {
                StaticInstances.ServerLogger.AppendError($"create project \"project_id\" parameter must have GUID format");
                return;
            }

            if (StaticInstances.ProjectsManager.ExistProject(directory))
            {
                StaticInstances.ServerLogger.AppendError($"project {directory} already appended");
                return;
            }

            var proj = new ServerProjectInfo(args, directory);

            StaticInstances.ProjectsManager.AddProject(proj);

            StaticInstances.ProjectsManager.SaveProjLibrary();


            StaticInstances.ServerLogger.AppendInfo($"project {proj.Info.Name} by id {proj.Info.Id} created");
        }

        protected void CreateUser(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Create user");

            if (!args.ContainsKey("name"))
            {
                StaticInstances.ServerLogger.AppendError($"create user must have \"name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                var user = new UserInfo(args);

                if (projectInfo.AddUser(user))
                    StaticInstances.ServerLogger.AppendInfo($"user {user.Name} by id {user.Id} created");
                else
                    StaticInstances.ServerLogger.AppendError($"user {user.Name} already exists");
            }
        }

        protected void AddUser(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Add user");

            if (!args.TryGetOutValue("path", out string path))
            {
                StaticInstances.ServerLogger.AppendError($"Add user must have \"path\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                var fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    StaticInstances.ServerLogger.AppendError($"{fileInfo.FullName} not exists");

                    return;
                }
                if (fileInfo.Extension != "priuk")
                {
                    StaticInstances.ServerLogger.AppendError($"{fileInfo.FullName} must have .priuk extension");

                    return;
                }

                var dest = Path.Combine(projectInfo.UsersDirPath, fileInfo.Name);

                File.Copy(path, dest, true);

                StaticInstances.ServerLogger.AppendError($"{fileInfo.FullName} private key copied to {projectInfo.Info.Name} project ({dest})");
            }
        }

        protected void AddPatchConnection(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Add Patch Connection");

            if (!args.TryGetOutValue("ip_address", out string ip_address))
            {
                StaticInstances.ServerLogger.AppendError($"Add Patch Connection must have \"ip_address\" parameter");
                return;
            }

            if (!args.TryGetOutValue("port", out ushort port))
            {
                StaticInstances.ServerLogger.AppendError($"Add Patch Connection must have \"port\" parameter");
                return;
            }

            if (!args.TryGetOutValue("input_cipher_key", out string input_cipher_key))
            {
                input_cipher_key = StaticInstances.ServerConfiguration.GetValue<string>("server.io.output.key");

                StaticInstances.ServerLogger.AppendInfo($"Not contains \"input_cipher_key\" parameter. Set from configuration {input_cipher_key}");
            }

            if (!args.TryGetOutValue("output_cipher_key", out string output_cipher_key))
            {
                output_cipher_key = StaticInstances.ServerConfiguration.GetValue<string>("server.io.input.key");

                StaticInstances.ServerLogger.AppendInfo($"Not contains \"output_cipher_key\" parameter. Set from configuration {output_cipher_key}");
            }

            if (!args.TryGetOutValue("identity_name", out string identity_name))
            {
                StaticInstances.ServerLogger.AppendError($"Add Patch Connection must have \"identity_name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                projectInfo.UpdatePatchInfo(new ProjectPatchInfo()
                {
                    IpAddress = ip_address,
                    Port = (int)port,
                    InputCipherKey = input_cipher_key,
                    OutputCipherKey = output_cipher_key,
                    SignName = identity_name
                });

                StaticInstances.ServerLogger.AppendInfo($"Patch connection info changed in {projectInfo.Info.Name}({projectInfo.Info.Id}) project");
            }
        }

        protected void CloneIdentity(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Clone identity");

            if (!args.TryGetOutValue("source_project_id", out string sourceProjectId))
            {
                StaticInstances.ServerLogger.AppendError($"Clone identity must have \"source_project_id\" parameter");
                return;
            }

            ServerProjectInfo pidest = GetProject(args);

            if (pidest != null)
            {
                ServerProjectInfo pisrc = StaticInstances.ProjectsManager.GetProject(sourceProjectId);

                if (pisrc == null)
                {
                    StaticInstances.ServerLogger.AppendError($"project by source_project_id = {sourceProjectId} not found");

                    return;
                }

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

                    StaticInstances.ServerLogger.AppendError($"{priKeyCount} private and {pubKeyCount} public keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");

                    return;
                }

                StaticInstances.ServerLogger.AppendError($"{priKeyCount} private keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");
            }
        }

        protected void DevClearInvalidPath(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Try DevClearInvalidPath");

            foreach (var item in StaticInstances.ProjectsManager.GetProjects())
            {
                StaticInstances.ServerLogger.AppendInfo($"Start process {item.Info.Name}");
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

                StaticInstances.ServerLogger.AppendInfo($"Cleared {c}");

                item.ReIndexing();
            }   
        }

        public bool Process()
        {
            CommandLineArgs args = new CommandLineArgs();

            if (args["action"] == default)
            {
                StaticInstances.ServerLogger.AppendInfo("Commands is empty");
                return false;
            }
            if (!commands.TryGetValue(args["action"], out var action))
            {
                StaticInstances.ServerLogger.AppendInfo($"Command not found {args["action"]}");
                return true;
            }
            StaticInstances.CommandExecutor = true;

            ServerOptions<PublisherNetworkClient> options = new ServerOptions<PublisherNetworkClient>();

            options.HelperLogger = StaticInstances.ServerLogger;

            options.LoadManagers<PublisherNetworkClient>(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));

            action(args);

            return true;
        }
    }
}
