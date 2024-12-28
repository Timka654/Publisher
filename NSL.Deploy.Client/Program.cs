using Newtonsoft.Json;
using NSL.Deploy.Client.Utils.Commands;
using NSL.ServiceUpdater.Shared;
using NSL.Utils.CommandLine;
using NSL.Utils.CommandLine.CLHandles;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerPublisher.Client
{
    class Program
    {
        public static ConfigurationInfoModel Configuration { get; private set; } = new ConfigurationInfoModel();

        public static string ConfigurationPath { get; private set; }

        #region Updater

        static async Task LoadUpdater(string appPath)
        {
            var updateFilePath = Path.Combine(appPath, "nsl_version.json");

            if (!File.Exists(updateFilePath))
                return;

            if (await UpdateChecker.CheckStartUpdateBaseScenario())
            {
                if (await UpdateChecker.CheckUpdate(updateFilePath, configurePostprocessing: configureVersionHandle, exception: exceptionVersionHandle, createIfDoesNotExists: true))
                {
                    Console.WriteLine("Update started...");
                }
                else
                    Console.WriteLine("Cannot found any updates... Try again/later");

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
            config.UpdateVersion("update1", c => c
            .SetValue(() => c.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployclient/")
            );
        }

        #endregion

        static void LoadConfiguration(string appPath)
        {
            ConfigurationPath = Path.Combine(appPath, "config.json");

            if (File.Exists(ConfigurationPath))
                Configuration = JsonConvert.DeserializeObject<ConfigurationInfoModel>(File.ReadAllText(ConfigurationPath));
            else
                Configuration = new ConfigurationInfoModel();
        }


        static async Task Main(string[] args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            LoadConfiguration(appPath);

            await LoadUpdater(appPath);

            await CLHandler<AppCommands>.Instance
                .ProcessCommand(new CommandLineArgs().CreateReader());
        }
    }
}
