﻿using Newtonsoft.Json;
using NSL.Deploy.Client.Utils.Commands;
using NSL.ServiceUpdater.Shared;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerPublisher.Client
{
    class Program
    {
        private static ConfigurationInfoModel Configuration { get; set; } = new ConfigurationInfoModel();

        public static string TemplatesPath => ExpandPath(Configuration.TemplatesPath);

        public static string KeysPath => ExpandPath(Configuration.KeysPath);

        public static string AppDataFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "deployclient");

        private static UpdaterConfig updaterConfig;

        public static string Version => updaterConfig?.CurrentVersion;

        public static string ConfigurationPath { get; private set; }

        #region Updater

        static async Task LoadUpdater(string appPath)
        {
            var updateFilePath = Path.Combine(appPath, "nsl_version.json");

            if (!File.Exists(updateFilePath))
                return;

            if (await UpdateChecker.CheckStartUpdateBaseScenario())
            {
                if (await UpdateChecker.CheckUpdate(updateFilePath, configurePostprocessing: configureVersionHandle, buildContext: context =>
                {
                    context.OnChangeCheckState = state =>
                    {
                        switch (state)
                        {
                            case UpdaterCheckStepEnum.Start:
                                Console.WriteLine($"Version checking starting...");
                                break;
                            case UpdaterCheckStepEnum.Finish:
                                Console.WriteLine($"Check version finish");
                                break;
                            case UpdaterCheckStepEnum.NewVersionDetected:
                                Console.WriteLine($"Detected new version");
                                break;
                            case UpdaterCheckStepEnum.NotAnyNewVersion:
                                Console.WriteLine($"No available new version");
                                break;
                            case UpdaterCheckStepEnum.FailedCheck:
                                Console.WriteLine($"Failed check version web request");
                                break;
                            default:
                                break;
                        }

                        return Task.CompletedTask;
                    };
                    context.OnChangeDownloadState = state =>
                    {
                        switch (state)
                        {
                            case UpdaterDownloadStepEnum.Start:
                                Console.WriteLine($"Starting download...");
                                break;
                            case UpdaterDownloadStepEnum.StartDownloadingUpdater:
                                Console.WriteLine($"Download updater started...");
                                break;
                            case UpdaterDownloadStepEnum.FinishDownloadingUpdater:
                                Console.WriteLine($"Download updater finished");
                                break;
                            case UpdaterDownloadStepEnum.FailedDownloadingUpdater:
                                Console.WriteLine($"Failed download updater web request");
                                break;
                            case UpdaterDownloadStepEnum.StartDownloadingVersion:
                                Console.WriteLine($"Download new version started...");
                                break;
                            case UpdaterDownloadStepEnum.FinishDownloadingVersion:
                                Console.WriteLine($"Download new version finished");
                                break;
                            case UpdaterDownloadStepEnum.FailedDownloadingVersion:
                                Console.WriteLine($"Failed download version web request");
                                break;
                            default:
                                break;
                        }

                        return Task.CompletedTask;
                    };
                    context.OnException = exceptionVersionHandle;
                    return Task.CompletedTask;
                }, createIfDoesNotExists: true))
                {
                    Console.WriteLine("Update started...");
                }
                else
                    Console.WriteLine("Cannot found any updates or have errors... Try again/later");

                return;
            }

            if (await UpdateChecker.CheckUpdatesBaseScenario(updateFilePath, configurePostprocessing: configureVersionHandle, createIfDoesNotExists: true))
            {
                await Task.Delay(3_000);
            }
        }

        private static async Task exceptionVersionHandle(UpdaterStepEnum step, Exception ex)
        {
            Console.WriteLine($"Version system have error on step {step.ToString()} - {ex.Message}");
        }

        private static void configureVersionHandle(UpdaterConfig config)
        {
            updaterConfig = config;

            if (config.UpdateUrl == "https://pubstorage.twicepricegroup.com/update/deployclient/")
                config.UpdateVersion("update1", c => c
                .SetValue(() => c.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployclient/")
                );
        }

        #endregion

        public static void InitData()
        {
            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);

            var configurationPath = Path.Combine(AppDataFolder, "config.json");

            if (!File.Exists(configurationPath))
                File.WriteAllText(configurationPath, JsonConvert.SerializeObject(Configuration));

            if (!Directory.Exists(TemplatesPath))
                Directory.CreateDirectory(TemplatesPath);

            if (!Directory.Exists(KeysPath))
                Directory.CreateDirectory(KeysPath);

            var versionPath = Path.Combine(AppDataFolder, "nsl_version.json");

            var cfg = new UpdaterConfig();

            cfg
                .SetValue(() => cfg.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployclient/")
                .SetValue(() => cfg.IgnorePathPatterns = ["(\\s\\S)*config.json"])
                .SetValue(() => cfg.Log = false)
                .SetValue(() => cfg.ProcessKill = true)
                .Save(versionPath);
        }

        static void LoadConfiguration(string appPath)
        {
            ConfigurationPath = Path.Combine(appPath, "config.json");

            if (File.Exists(ConfigurationPath))
                Configuration = JsonConvert.DeserializeObject<ConfigurationInfoModel>(File.ReadAllText(ConfigurationPath));
        }


        static async Task Main(string[] args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                LoadConfiguration(AppDataFolder);

                await LoadUpdater(AppDataFolder);

                await CLHandler<AppCommands>.Instance
                    .ProcessCommand(new CommandLineArgs().CreateReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await Task.Delay(1_000);
        }

        private static string ExpandPath(string path)
        {
            path = path.Replace("%APPLICATIONAPPDATA%", AppDataFolder);

            path = Environment.ExpandEnvironmentVariables(path);

            return path;
        }
    }
}
