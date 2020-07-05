using Publisher.Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client.Tools
{
    public class Commands
    {
        private static readonly Dictionary<string, Action<CommandLineArgs>> commands = new Dictionary<string, Action<CommandLineArgs>>()
        {
            { "publish", (new Publish()).Run },
        };

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

            action(args);

            return true;
        }
    }
}
