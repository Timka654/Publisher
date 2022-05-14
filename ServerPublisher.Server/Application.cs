using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServerPublisher.Server
{
    public class Application
    {
        public static string Directory { get; private set; }

        static Application()
        {
            Directory = ProcessDirectory();
        }

        private static string ProcessDirectory()
        {
            if (Debugger.IsAttached)
                return System.IO.Directory.GetCurrentDirectory();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Environment.CurrentDirectory;

            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
