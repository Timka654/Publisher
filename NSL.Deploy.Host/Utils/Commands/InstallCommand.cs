using ServerPublisher.Server.Info;
using System;
using System.IO;
using NSL.Logger;
using ServerPublisher.Shared.Utils;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System.Threading.Tasks;
using ServerPublisher.Server.Utils;
using NSL.Utils.CommandLine.CLHandles.Arguments;

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
    internal class InstallCommand : CLHandler
    {
        public override string Command => "install";

        public override string Description { get => ""; set => base.Description = value; }

        public InstallCommand()
        {
            AddArguments(SelectArguments());
        }

        public override async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
        {
            base.ProcessingAutoArgs(values);

            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            bool isDefault = values.ContainsArg("default");

            bool isService = values.ContainsArg("service");

            bool reInit = values.ContainsArg("reinit");

            bool haveServiceName = values.TryGetValue("service_name", out string serviceName, "Deploy Host");
            bool haveServicePath = values.TryGetValue("service_file_name", out string serviceFileName, "deployhost.service");


            if (!values.TryGetValue("path", out string path))
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

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    if (!haveServicePath)
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

            var configPath = Path.Combine(path, "ServerSettings.json").GetNormalizedPath();


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

            if (isService)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    this.TerminalExecute($"systemctl disable {serviceFileName}");
                    this.TerminalExecute($"systemctl stop {serviceFileName}");
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

                    AppCommands.Logger.AppendInfo($"Copy '{item}' -> '{ePath}'");

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

            if (Environment.OSVersion.Platform == PlatformID.Unix)
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

            return CommandReadStateEnum.Success;
        }
    }
}
