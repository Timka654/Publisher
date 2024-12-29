using NSL.Deploy.Host.Utils.Commands;
using NSL.Logger;
using NSL.ServiceUpdater.Shared;
using NSL.Utils.CommandLine.CLHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerPublisher.Server
{
    public class Program
    {

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

            UpdateChecker.Instance.RunChecker(updateFilePath, configurePostprocessing: configureVersionHandle, createIfDoesNotExists: true);
        }

        private static async Task exceptionVersionHandle(UpdaterStepEnum step, Exception ex)
        {
            Console.WriteLine($"Version system have error on step {step.ToString()} - {ex.Message}");
        }

        private static void configureVersionHandle(UpdaterConfig config)
        {
            config.UpdateVersion("update1", c => c
            .SetValue(() => c.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployserver/")
            );
        }

        #endregion

        static async Task Main(string[] args)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            await LoadUpdater(appPath);

            PublisherServer.InitializeApp(appPath);

            await CLHandler<AppCommands>.Instance
                .ProcessCommand(new NSL.Utils.CommandLine.CommandLineArgsReader(new NSL.Utils.CommandLine.CommandLineArgs()));
        }
    }
}
