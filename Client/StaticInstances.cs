using SCLogger;
using System;
using System.IO;

namespace Publisher.Client
{
    public class StaticInstances
    {
        public static FileLogger ServerLogger { get; } = FileLogger.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/client"));
    }
}
