using NSL.Logger;
using ServerPublisher.Server.Utils;
using System;

namespace ServerPublisher.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            PublisherServer.InitializeApp();

            if (Commands.Process())
                return;

            PublisherServer.ServerLogger.AppendError($"Unknown args {string.Join(" ", args)}");
        }
    }
}
