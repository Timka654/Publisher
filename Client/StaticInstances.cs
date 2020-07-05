using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Client
{
    public class StaticInstances
    {
        public static FileLogger ServerLogger { get; } = FileLogger.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/client"));
    }
}
