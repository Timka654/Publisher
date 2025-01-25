using NSL.Deploy.Host.Utils.Commands;
using NSL.ServerOptions.Extensions.Manager;
using NSL.ServiceUpdater.Shared;
using NSL.Utils.CommandLine.CLHandles;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServerPublisher.Server
{
    public class Program
    {

        #region Updater

        static async Task LoadUpdater(string appPath)
        {
            var updateFilePath = Path.Combine(appPath, "data", "nsl_version.json");

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
            if (config.ConfigurationVersion == "initial")
                config.UpdateVersion("update1", c => c
                .SetValue(() => c.UpdateUrl = "https://pubstorage.mtvworld.net/update/deployhost/")
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

#if DEBUG

            Console.ReadKey();
#endif
        }
    }
}
