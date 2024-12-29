using Newtonsoft.Json;
using NSL.Logger;
using NSL.Logger.Interface;
using NSL.ServiceUpdater.Shared;
using NSL.Utils.CommandLine.CLHandles;
using ServerPublisher.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace NSL.Deploy.Client.Utils.Commands
{
    internal class AppCommands : CLHandler
    {
        public static ILogger Logger { get; } = ConsoleLogger.Create();

        public AppCommands()
        {
            AddCommands(SelectSubCommands<CLHandleSelectAttribute>("default", true));
        }
    }
}
