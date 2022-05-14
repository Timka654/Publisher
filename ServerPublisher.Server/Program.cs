using ServerPublisher.Server.Utils;
using System;

namespace ServerPublisher.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            if ((new Commands()).Process())
                return;

            Console.WriteLine($"Unknown args {string.Join(" ", args)}");
        }
    }
}
