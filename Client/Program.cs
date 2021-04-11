using Publisher.Client;
using Publisher.Client.Tools;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Commands.Process();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (StaticInstances.ServerLogger?.Initialized == true)
            {
                StaticInstances.ServerLogger.Flush();
            }
        }
    }
}
