using ServerPublisher.Server.Info;
using System;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using Newtonsoft.Json;
using System.Diagnostics;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server.Utils;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using NSL.ServiceUpdater.Shared;

namespace NSL.Deploy.Host.Utils.Commands
{
    [CLHandleSelect("default")]
    [CLArgument("default", typeof(CLContainsType), true, Description = "Execute command with default parameters")]
    [CLArgument("service", typeof(CLContainsType), true, Description = "Configure service after copy files")]
    [CLArgument("reinit", typeof(CLContainsType), true, Description = "Reconfigure current instance in this path without copy files")]
    [CLArgument("path", typeof(string), true, Description = "Directory path for install")]
    [CLArgument("service_name", typeof(string), true, Description = "Service name for register and manage")]
    [CLArgument("service_file_name", typeof(string), true, Description = "(Linux only*) Service filename")]
    [CLArgument("q", typeof(CLContainsType), true, Description = "Close console after install")]
    [CLArgument("y", typeof(CLContainsType), true)]
    [CLArgument("flags", typeof(string), true)]
    internal class InstallCommand : CLHandler
    {
        public override string Command => "install";

        public override string Description { get => ""; set => base.Description = value; }

        public InstallCommand()
        {
            AddArguments(SelectArguments());
        }

        [CLArgumentExists("default")] bool isDefault { get; set; }

        [CLArgumentExists("service")] bool isService { get; set; }

        [CLArgumentExists("reinit")] bool reInit { get; set; }

        [CLArgumentValue("path")] string? path { get; set; }

        [CLArgumentExists("path")] bool havePath { get; set; }

        [CLArgumentValue("service_name", "Deploy Host")] string? serviceName { get; set; }

        [CLArgumentExists("service_name")] bool haveServiceName { get; set; }

        [CLArgumentValue("service_file_name", "deployhost.service")] string? serviceFileName { get; set; }

        [CLArgumentExists("service_file_name")] bool haveServiceFileName { get; set; }


        [CLArgumentExists("q")] bool quit { get; set; }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            bool isLinuxPlatform = Environment.OSVersion.Platform == PlatformID.Unix;

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            if (!havePath)
            {
                if (isLinuxPlatform)
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
                    path = CommandParameterReader.Read("Install directory", AppCommands.Logger, path);
            }

            if (reInit)
                path = appPath;

            if (isService)
            {
                if (!haveServiceName)
                {
                    if (!isDefault)
                        serviceName = CommandParameterReader.Read("Service name", AppCommands.Logger, serviceName);
                }

                if (isLinuxPlatform)
                {
                    if (!haveServiceFileName)
                    {
                        if (!isDefault)
                            serviceFileName = CommandParameterReader.Read("Service file name", AppCommands.Logger, serviceFileName);
                    }
                }
            }

            if (!values.TryGetValue("port", out var port, 6583))
            {
                if (!isDefault)
                    port = CommandParameterReader.Read("Server port", AppCommands.Logger, port);
            }

            var execPath = Path.Combine(path, "deployhost");

            if (Environment.OSVersion.Platform != PlatformID.Unix)
                execPath += ".exe";

            var configPath = Path.Combine(path, "data", "ServerSettings.json").GetNormalizedPath();


            AppCommands.Logger.AppendInfo($"""

- current path: {appPath}
- destination path: {path}
- config path: {configPath}
- is service: {isService}
- service name: {serviceName}
- service path(linux only): {serviceFileName}
- port: {port}
""");

            if (!values.ConfirmCommandAction(AppCommands.Logger))
                return CommandReadStateEnum.Failed;

            if (!Directory.Exists(path) && !reInit)
                Directory.CreateDirectory(path);

            var dataDirPath = Path.GetFullPath(Path.Combine(path, "data"));

            if (isService)
            {
                if (isLinuxPlatform)
                {
                    this.TerminalExecute($"systemctl disable {serviceFileName}");
                    this.TerminalExecute($"systemctl stop {serviceFileName}");
                }
                else
                {

                }
            }

            if(!Directory.Exists(dataDirPath))
                Directory.CreateDirectory(dataDirPath);

            if (!reInit)
            {
                var versionPath = Path.Combine(dataDirPath, "nsl_version.json");

                var cfg = new UpdaterConfig();

                if (isService)
                {
                    cfg
                        .SetValue(() => cfg.ServiceName = isLinuxPlatform ? Path.GetFileName(serviceFileName) : serviceName)
                        .SetValue(() => cfg.ServiceStopping = true)
                        .SetValue(() => cfg.ServiceRestarting = true);
                }

                cfg
                    .SetValue(()=> cfg.ConfigurationVersion = "initial")
                    .SetValue(() => cfg.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployhost/")
                    .SetValue(() => cfg.Log = false)
                    .SetValue(() => cfg.ProcessKill = false)
                    .Save(versionPath);


                foreach (var item in Directory.GetFiles(appPath, "*", SearchOption.AllDirectories))
                {
                    int i = 0;

                    var ePath = Path.GetFullPath(Path.Combine(path, Path.GetRelativePath(appPath, item)));

                    var dir = Path.GetDirectoryName(ePath);

                    AppCommands.Logger.AppendInfo($"Copy '{item}' -> '{ePath}'");

                    if ((ePath.StartsWith(dataDirPath)) && File.Exists(ePath))
                    {
                        AppCommands.Logger.AppendError($"'{ePath}' already exists - skip");
                        continue;
                    }

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
                            AppCommands.Logger.AppendError(ex.ToString());

                            i++;

                            if (i == 5)
                                return CommandReadStateEnum.Failed;

                            await Task.Delay(1_000);

                            continue;
                        }

                        break;

                    } while (true);
                }
            }

            if (isLinuxPlatform)
            {
                this.TerminalExecute("rm /bin/deployhost");
                this.TerminalExecute($"ln -s \\\"{execPath}\\\" /bin/deployhost");

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
                if (isLinuxPlatform)
                {
                    File.WriteAllText(Path.Combine("/etc/systemd/system/", serviceFileName), $"""
[Unit]
Description={serviceName}

[Service]
WorkingDirectory={path}
ExecStart={execPath} service
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
KillMode=process

[Install]
WantedBy=multi-user.target
""");

                    this.TerminalExecute($"systemctl enable {serviceFileName}");

                    AppCommands.Logger.AppendInfo($"Service \"{serviceName}\" enabled, print \"systemctl start {serviceFileName}\" for start now");
                }
                else
                {
                    Process.Start("sc.exe", $"create \"{serviceName}\" binPath=\"{execPath} / action:service\"\"\" start=auto");
                }
            }

            if (!quit)
            {
                AppCommands.Logger.AppendInfo("Press any key to continue...");

                Console.ReadKey();
            }

            return CommandReadStateEnum.Success;
        }
    }
}
