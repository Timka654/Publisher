﻿using Publisher.Basic;
using System;
using System.Collections.Generic;

namespace Publisher.Client.Tools
{
    public class Commands
    {
        private static readonly Dictionary<string, Action<CommandLineArgs>> commands = new Dictionary<string, Action<CommandLineArgs>>()
        {
            { "publish", (cmd) => (new Publish()).Run(cmd).ConfigureAwait(true).GetAwaiter().GetResult() },
        };

        public static bool Process()
        {
            CommandLineArgs args = new CommandLineArgs();

            if (args["action"] == default)
            {
                Console.WriteLine("Commands is empty");
                return false;
            }
            if (!commands.TryGetValue(args["action"], out var action))
            {
                Console.WriteLine($"Command not found {args["action"]}");
                return true;
            }

            action(args);

            return true;
        }
    }
}
