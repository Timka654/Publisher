using Publisher.Basic;
using Publisher.Server.Info;
using Publisher.Server.Network;
using ServerOptions.Extensions.Manager;
using SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Remoting;
using System.Reflection;

namespace Publisher.Server.Tools
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
                { "clone_identity", CloneIdentity },
                { "check_scripts", CheckScripts }
            };
        }


        protected void CheckScripts(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Check Scripts");

            if (args.ContainsKey("project_id") == false || !Guid.TryParse(args["project_id"], out var pid))
            {
                StaticInstances.ServerLogger.AppendError($"check project scripts \"project_id\" parameter must have GUID format");
                return;
            }
            var pi = StaticInstances.ProjectsManager.GetProject(args["project_id"]);

            if (pi == null)
            {
                StaticInstances.ServerLogger.AppendError($"check project scripts \"{args["project_id"]}\" not found");
                return;
            }

            pi.CheckScripts();
        }

            protected void CreateProject(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Create project");

            if (!args.ContainsKey("directory"))
            {
                StaticInstances.ServerLogger.AppendError($"create project must have \"directory\" parameter");
                return;
            }
            if (!args.ContainsKey("name"))
            {
                StaticInstances.ServerLogger.AppendError($"create project must have \"name\" parameter");
                return;
            }
            if (args.ContainsKey("project_id") && !Guid.TryParse(args["project_id"], out var pid))
            {
                StaticInstances.ServerLogger.AppendError($"create project \"project_id\" parameter must have GUID format");
                return;
            }

            if (StaticInstances.ProjectsManager.ExistProject(args["directory"]))
            {
                StaticInstances.ServerLogger.AppendError($"project {args["directory"]} already appended");
                return;
            }

            var proj = new ProjectInfo(args);

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

            ProjectInfo pi = null;

            if (args.ContainsKey("project_id"))
                pi = StaticInstances.ProjectsManager.GetProject(args["project_id"]);
            if (pi == null && args.ContainsKey("project_name"))
                pi = StaticInstances.ProjectsManager.GetProjectByName(args["project_name"]);

            if (pi == null)
            {
                if (args.ContainsKey("project_id") && args.ContainsKey("project_name"))
                {
                    StaticInstances.ServerLogger.AppendError($"project by project_id = {args["project_id"]} or project_name = {args["project_name"]} not found");
                }
                else if (args.ContainsKey("project_id"))
                {
                    StaticInstances.ServerLogger.AppendError($"project by project_id = {args["project_id"]} not found");
                }
                else if (args.ContainsKey("project_name"))
                {
                    StaticInstances.ServerLogger.AppendError($"project by project_name = {args["project_name"]} not found");
                }
                else
                {
                    StaticInstances.ServerLogger.AppendError($"create user must have \"project_name\" or \"project_id\" parameter");
                }
                return;
            }

            var user = new UserInfo(args);


            if (pi.AddUser(user))
                StaticInstances.ServerLogger.AppendInfo($"user {user.Name} by id {user.Id} created");
        }

        protected void AddUser(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Add user");
            if (!args.ContainsKey("project_id"))
            {
                StaticInstances.ServerLogger.AppendError($"Add user must have \"project_id\" parameter");
                return;
            }

            if (!args.ContainsKey("path"))
            {
                StaticInstances.ServerLogger.AppendError($"Add user must have \"path\" parameter");
                return;
            }

            ProjectInfo pi = StaticInstances.ProjectsManager.GetProject(args["project_id"]);

            if (pi == null)
            {
                StaticInstances.ServerLogger.AppendError($"project by project_id = {args["project_id"]} not found");
                return;
            }

            var f = new FileInfo(args["path"]);

            if (f.Extension != "priuk")
            {
                StaticInstances.ServerLogger.AppendError($"{f.FullName} must have .priuk extension");
                return;
            }

            var dest = Path.Combine(pi.UsersDirPath, f.Name);

            File.Copy(args["path"], dest);

            StaticInstances.ServerLogger.AppendError($"{f.FullName} private key copied to {pi.Info.Name} project ({dest})");
        }

        protected void CloneIdentity(CommandLineArgs args)
        {
            StaticInstances.ServerLogger.AppendInfo("Clone identity");

            if (!args.ContainsKey("source_project_id"))
            {
                StaticInstances.ServerLogger.AppendError($"Clone identity must have \"source_project_id\" parameter");
                return;
            }

            if (!args.ContainsKey("destination_project_id"))
            {
                StaticInstances.ServerLogger.AppendError($"Clone identity must have \"destination_project_id\" parameter");
                return;
            }

            ProjectInfo pisrc = StaticInstances.ProjectsManager.GetProject(args["source_project_id"]);
            ProjectInfo pidest = StaticInstances.ProjectsManager.GetProject(args["destination_project_id"]);

            if (pisrc == null || pidest == null)
            {
                if (pisrc == null)
                {
                    StaticInstances.ServerLogger.AppendError($"project by source_project_id = {args["source_project_id"]} not found");
                }
                else
                {
                    StaticInstances.ServerLogger.AppendError($"project by destination_project_id = {args["destination_project_id"]} not found");
                }

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
