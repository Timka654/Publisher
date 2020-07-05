using Publisher.Server.Info;
using Publisher.Server.Managers;
using Publisher.Server.Network;
using ServerOptions.Extensions.Manager;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Server.Tools
{
    public class Commands
    {
        private static readonly Dictionary<string, Action<CommandLineArgs>> commands = new Dictionary<string, Action<CommandLineArgs>>()
        {
            { "create_project", CreateProject },
            { "create_user", CreateUser }
        };


        private static void CreateProject(CommandLineArgs args)
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

        private static void CreateUser(CommandLineArgs args)
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
                    return;
                }
                return;
            }

            var user = new UserInfo(args);


            if (pi.AddUser(user))
                StaticInstances.ServerLogger.AppendInfo($"user {user.Name} by id {user.Id} created");
        }

        public static bool Process()
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

            ServerOptions<NetworkClient> options = new ServerOptions<NetworkClient>();

            options.HelperLogger = StaticInstances.ServerLogger;

            options.LoadManagers(Assembly.GetExecutingAssembly(), typeof(ManagerLoadAttribute));

            action(args);

            return true;
        }
    }
}
