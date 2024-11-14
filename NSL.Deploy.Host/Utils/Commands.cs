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
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace ServerPublisher.Server.Utils
{
    public class Commands
    {
        private record struct Command(Action<CommandLineArgs> action, string helpContent, string helpDetailsContent);

        static Dictionary<string, Command> commands = new()
        {
            { "install", new Command(Install,"","") },
            { "service", new Command(RunService,"","") },
            { "cset", new Command(ConfigurationSet,"","") },
            { "cdefault", new Command(ConfigurationDefault,"","") },
            { "create_project", new Command(CreateProject,"","") },
            { "update_project", new Command(UpdateProject,"","") },
            { "link_project", new Command(LinkProject,"","") },
            { "create_user", new Command(CreateUser,"","") },
            { "add_user", new Command(AddUser,"","") },
            { "add_patch_connection", new Command(AddPatchConnection,"","") },
            { "clone_identity", new Command(CloneIdentity,"","") },
            { "check_scripts", new Command(CheckScripts,"","") },
            { "reindexing", new Command(ReIndexing,"","") },
            { "dev_clear_invalid_path", new Command(DevClearInvalidPath,"","") }
        };

        static void Install(CommandLineArgs args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            bool isDefault = args.ContainsKey("default");

            bool isService = args.ContainsKey("service");

            bool reInit = args.ContainsKey("reinit");

            string serviceName = "Deploy Host";

            string serviceFileName = "deployhost.service";

            if (!args.TryGetOutValue("path", out string path))
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    path = "/etc/deployhost";
                }
                else
                {
                    if (Environment.Is64BitProcess)
                        path = @"C:\Program Files (x86)\DeployHost";
                    else
                        path = @"C:\Program Files\DeployHost";
                }

                if (!isDefault && !reInit)
                    path = CommandParameterReader.Read("Install directory", Logger, path);
            }

            if (reInit)
                path = appPath;

            if (isService)
            {
                if (!args.TryGetValue("service_name", ref serviceName))
                {
                    if (!isDefault)
                        serviceName = CommandParameterReader.Read("Service name", Logger, serviceName);
                }

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    if (!args.TryGetValue("service_file_name", ref serviceFileName))
                    {
                        if (!isDefault)
                            serviceFileName = CommandParameterReader.Read("Service file name", Logger, serviceFileName);
                    }
                }
            }

            int port = 6583;

            if (!args.TryGetValue("port", ref port))
            {
                if (!isDefault)
                    port = CommandParameterReader.Read("Server port", Logger, port);
            }

            var execPath = Path.Combine(path, "deployhost");

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                execPath += ".exe";

            var configPath = Path.Combine(path, "ServerSettings.json").GetNormalizedPath();


            Logger.AppendInfo($"""

- current path: {appPath}
- destination path: {path}
- config path: {configPath}
- is service: {isService}
- service name: {serviceName}
- service path(linux only): {serviceFileName}
- port: {port}
""");

            if (!args.ConfirmAction(Logger))
                return;

            if (!Directory.Exists(path) && !reInit)
                Directory.CreateDirectory(path);

            if (isService)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    TerminalExecute($"systemctl disable {serviceFileName}");
                    TerminalExecute($"systemctl stop {serviceFileName}");
                }
                else
                {

                }
            }

            if (!reInit)
            {
                foreach (var item in Directory.GetFiles(appPath, "*", SearchOption.AllDirectories))
                {
                    int i = 0;

                    var ePath = Path.Combine(path, Path.GetRelativePath(appPath, item));

                    var dir = Path.GetDirectoryName(ePath);

                    Logger.AppendInfo($"Copy '{item}' -> '{ePath}'");

                    do
                    {
                        try
                        {
                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            File.Delete(ePath);

                            File.Copy(item, ePath, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.AppendError(ex.ToString());

                            i++;

                            if (i == 5)
                                return;

                            Thread.Sleep(1_000);

                            continue;
                        }

                        break;

                    } while (true);
                }
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                TerminalExecute("rm /bin/deployhost");
                TerminalExecute($"ln -s \\\"{execPath}\\\" /bin/deployhost");

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

            var config = new ConfigurationSettingsInfo();

            if (File.Exists(configPath))
            {
                config = JsonConvert.DeserializeObject<ConfigurationSettingsInfo>(File.ReadAllText(configPath));
            }

            config.Publisher.Server.IO.Port = port;

            File.WriteAllText(configPath, JsonConvert.SerializeObject(config, JsonUtils.JsonSettings));

            if (isService)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    File.WriteAllText(Path.Combine("/etc/systemd/system/", serviceFileName), $"""
[Unit]
Description={serviceName}

[Service]
WorkingDirectory={path}
ExecStart={execPath} /action:service
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
""");

                    TerminalExecute($"systemctl enable {serviceFileName}");

                    Logger.AppendInfo($"Service \"{serviceName}\" enabled, print \"systemctl start {serviceFileName}\" for start now");
                }
                else
                {
                    System.Diagnostics.Process.Start("sc.exe", $"create \"{serviceName}\" binPath=\"{execPath} / action:service\"\"\" start=auto");
                }
            }

        }


        static void RunService(CommandLineArgs args)
            => PublisherServer.RunServer();

        static void CheckScripts(CommandLineArgs args)
        {
            Logger.AppendInfo("Check Scripts");


            if (!args.ConfirmAction(Logger))
                return;

            GetProject(args)?.CheckScripts();
        }

        static void ReIndexing(CommandLineArgs args)
        {
            Logger.AppendInfo("Try reindexing");

            if (!args.ConfirmAction(Logger))
                return;

            GetProject(args)?.ReIndexing();
        }

        #region Project

        static void CreateProject(CommandLineArgs args)
        {
            Logger.AppendInfo("Create project");

            if (args.CheckHaveCommandFlag(Logger, "template") || args.CheckHaveCommandFlag(Logger, "template_path"))
            {
                UpdateProject(args);

                return;
            }

            GetDirParameter(args, "directory", out string directory);

            if (!TryGetCommandValue<string>(args, "name", out _))
            {
                Logger.AppendError($"create project must have \"name\" parameter");
                return;
            }

            if (TryGetCommandValue(args, "project_id", out string projectId) && !Guid.TryParse(projectId, out var _))
            {
                Logger.AppendError($"create project \"project_id\" parameter must have GUID format");
                return;
            }

            if (PublisherServer.ProjectsManager.ExistProject(directory))
            {
                Logger.AppendError($"project {directory} already appended");
                return;
            }

            if (!args.ConfirmAction(Logger))
                return;

            var proj = new ServerProjectInfo(args, directory);

            PublisherServer.ProjectsManager.AddProject(proj);

            PublisherServer.ProjectsManager.SaveProjLibrary();


            Logger.AppendInfo($"project {proj.Info.Name} by id {proj.Info.Id} created");
        }

        static void UpdateProject(CommandLineArgs args)
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

        static void LinkProject(CommandLineArgs args)
        {
            Logger.AppendInfo("Link project");

            GetDirParameter(args, "directory", out string directory);

            if (!args.ConfirmAction(Logger))
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

                    if (!args.ConfirmAction(Logger))
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

        #endregion

        static void CreateUser(CommandLineArgs args)
        {
            Logger.AppendInfo("Create user");

            if (!TryGetCommandValue<string>(args, "name", out _))
            {
                Logger.AppendError($"create user must have \"name\" parameter");
                return;
            }

            if (args.CheckHaveCommandFlag(Logger, "global"))
            {
                Logger.AppendInfo("Create global user");

                if (args.CheckHaveCommandFlag(Logger, "publisher"))
                {
                    Logger.AppendInfo("Create publisher user");

                    if (!args.ConfirmAction(Logger))
                        return;

                    var user = UserInfo.CreateUser(args);

                    if (PublisherServer.ProjectsManager.GlobalPublishUserStorage.AddUser(user))
                        Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        Logger.AppendError($"{user.Name} already exist");
                    }

                }
                else if (args.CheckHaveCommandFlag(Logger, "proxy"))
                {
                    Logger.AppendInfo("Create proxy user");

                    if (!args.ConfirmAction(Logger))
                        return;

                    var user = UserInfo.CreateUser(args);

                    if (PublisherServer.ProjectsManager.GlobalProxyUserStorage.AddUser(user))
                        Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        Logger.AppendError($"{user.Name} already exist");
                    }
                }
                else if (args.CheckHaveCommandFlag(Logger, "both"))
                {
                    Logger.AppendInfo("Create publisher/proxy user");

                    if (!args.ConfirmAction(Logger))
                        return;

                    var user = UserInfo.CreateUser(args);

                    if (PublisherServer.ProjectsManager.GlobalBothUserStorage.AddUser(user))
                        Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                    else
                    {
                        Logger.AppendError($"{user.Name} already exist");
                    }
                }


                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                if (!args.ConfirmAction(Logger))
                    return;

                var user = UserInfo.CreateUser(args);

                if (projectInfo.AddUser(user))
                    Logger.AppendInfo($"user {user.Name} by id {user.Id} success created");
                else
                {
                    Logger.AppendError($"{user.Name} already exist in project {projectInfo.Info.Name}({projectInfo.Info.Id})");
                }
            }
        }

        static void AddUser(CommandLineArgs args)
        {
            Logger.AppendInfo("Add user");

            if (!TryGetCommandValue(args, "path", out string path))
            {
                Logger.AppendError($"Add user must have \"path\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {

                if (!args.ConfirmAction(Logger))
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

        static void CloneIdentity(CommandLineArgs args)
        {
            Logger.AppendInfo("Clone identity");

            if (!TryGetCommandValue(args, "source_project_id", out string sourceProjectId))
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

                if (!args.ConfirmAction(Logger))
                    return;

                var files = new DirectoryInfo(pisrc.UsersDirPath).GetFiles("*.priuk");

                var priKeyCount = files.Length;

                foreach (var item in files)
                {
                    item.CopyTo(Path.Combine(pidest.UsersDirPath, item.Name).GetNormalizedPath(), true);
                }

                if (!args.CheckHaveCommandFlag(Logger, "only_private"))
                {
                    files = new DirectoryInfo(pisrc.UsersPublicsDirPath).GetFiles("*.pubuk");

                    var pubKeyCount = files.Length;

                    foreach (var item in files)
                    {
                        item.CopyTo(Path.Combine(pidest.UsersPublicsDirPath, item.Name).GetNormalizedPath(), true);
                    }

                    Logger.AppendError($"{priKeyCount} private and {pubKeyCount} public keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");

                    return;
                }

                Logger.AppendError($"{priKeyCount} private keys copied from  {pisrc.Info.Name} to {pidest.Info.Name}");
            }
        }

        static void AddPatchConnection(CommandLineArgs args)
        {
            Logger.AppendInfo("Add Patch Connection");

            if (!TryGetCommandValue(args, "ip_address", out string ip_address))
            {
                Logger.AppendError($"Add Patch Connection must have \"ip_address\" parameter");
                return;
            }

            if (!TryGetCommandValue(args, "port", out ushort port))
            {
                Logger.AppendError($"Add Patch Connection must have \"port\" parameter");
                return;
            }

            if (!TryGetCommandValue(args, "input_cipher_key", out string input_cipher_key))
            {
                input_cipher_key = PublisherServer.Configuration.Publisher.Server.Cipher.OutputKey;

                Logger.AppendInfo($"Not contains \"input_cipher_key\" parameter. Set from configuration {input_cipher_key}");
            }

            if (!TryGetCommandValue(args, "output_cipher_key", out string output_cipher_key))
            {
                output_cipher_key = PublisherServer.Configuration.Publisher.Server.Cipher.InputKey;

                Logger.AppendInfo($"Not contains \"output_cipher_key\" parameter. Set from configuration {output_cipher_key}");
            }

            if (!TryGetCommandValue(args, "identity_name", out string identity_name))
            {
                Logger.AppendError($"Add Patch Connection must have \"identity_name\" parameter");
                return;
            }

            ServerProjectInfo projectInfo = GetProject(args);

            if (projectInfo != null)
            {
                if (!args.ConfirmAction(Logger))
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

        static void DevClearInvalidPath(CommandLineArgs args)
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

        static void ConfigurationSet(CommandLineArgs args)
        {
            Logger.AppendInfo("Configuration set value");

            if (!TryGetCommandValue<string>(args, "path", out var _path))
            {
                Logger.AppendError($"Configuration set must have \"path\" parameter");
                return;
            }

            if (!TryGetCommandValue<string>(args, "value", out var value))
            {
                Logger.AppendError($"Configuration set must have \"value\" parameter");
                return;
            }

            if (!args.ConfirmAction(Logger))
                return;



            var cpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json").GetNormalizedPath();

            ConfigurationSettingsInfo cdata = File.Exists(cpath) ? JsonConvert.DeserializeObject<ConfigurationSettingsInfo>(File.ReadAllText(cpath)) : new ConfigurationSettingsInfo();

            List<(PropertyInfo property, Type type, object value)> cmap = [
                (null, typeof(ConfigurationSettingsInfo), cdata)
            ];

            var tAttr = typeof(JsonPropertyAttribute);

            var path = _path.Split('/');

            int c = 0;

            foreach (var item in path)
            {
                ++c;
                var li = cmap.Last();
                var lt = li.type;

                var props = lt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => new
                    {
                        property = x,
                        pathName = x.GetCustomAttributes().Where(x => x is JsonPropertyAttribute).Select(x => ((JsonPropertyAttribute)x).PropertyName).SingleOrDefault()
                    }).ToArray();

                var nprop = props.FirstOrDefault(x =>
                (x.pathName != default && x.pathName.Equals(item, StringComparison.InvariantCultureIgnoreCase))
                || x.property.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));

                if (nprop == default)
                {
                    Logger.AppendError($"Cannot found path {item} partition for set. {string.Join(".", path.Take(c))}");
                    return;
                }

                cmap.Add((nprop.property, nprop.property.PropertyType, nprop.property.GetValue(li.value)));
            }

            var curr = cmap.Last();

            var currobj = cmap[cmap.Count - 2].value ?? cdata;

            object setValue = null;

            if (curr.type == typeof(string))
            {
                setValue = value;
            }
            else if (curr.type == typeof(decimal))
            {
                setValue = decimal.Parse(value);
            }
            else if (curr.type == typeof(Guid))
            {
                setValue = Guid.Parse(value);
            }
            else if (curr.type.IsPrimitive)
            {
                setValue = Convert.ChangeType(value, curr.type);
            }
            else
            {
                Logger.AppendError($"Cannot set value {value} to {curr.type} type");
                return;
            }

            curr.property.SetValue(currobj, setValue);


            File.WriteAllText(cpath, JsonConvert.SerializeObject(cdata, JsonUtils.JsonSettings));
        }

        static void ConfigurationDefault(CommandLineArgs args)
        {
            Logger.AppendInfo("Configuration reset to default");

            if (!args.ConfirmAction(Logger))
                return;

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerSettings.json").GetNormalizedPath(), JsonConvert.SerializeObject(new ConfigurationSettingsInfo(), JsonUtils.JsonSettings));
        }

        static FileLogger Logger => PublisherServer.ServerLogger;

        #region Utils

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

            action.action(args);

            return true;
        }

        static bool GetDirParameter(CommandLineArgs args, string name, out string value)
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

        static bool TryGetCommandValue<T>(CommandLineArgs args, string key, out T result)
        {
            if (args.TryGetOutValue(key, out result))
            {
                Logger.AppendInfo($"\"{key}\" = \"{result}\"");

                return true;
            }

            Logger.AppendInfo($"\"{key}\" = <none>");

            return false;
        }

        static ServerProjectInfo GetProject(CommandLineArgs args)
        {
            ServerProjectInfo projectInfo;

            if (TryGetCommandValue(args, "project_id", out string projectId))
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

        static void PidOrDirInfo()
        {
            Logger.AppendError($"Current command must have project_id(has GUID format) or directory parameters for identity project");
            Logger.AppendError($"You can not using identity parameters if executing command from directory contains project");
        }

        static void TerminalExecute(string command)
        {
            System.Diagnostics.Process.Start("/bin/bash", $"-c \"{command}\"");
        }

        #endregion
    }
}
