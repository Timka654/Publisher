using Publisher.Client;
using Publisher.Client.Tools;
using System;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            if (Commands.Process())
                while (true)
                {
                    Thread.Sleep(500);
                }
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
