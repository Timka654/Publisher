using System;
using System.Diagnostics;

namespace debug_console
{
    class Program
    {
        static void Main(string[] args)
        {

            Process p = Process.Start("/bin/bash", $"-c \"systemctl start publisher.service\"");
            p.WaitForExit();



            var process = System.Diagnostics.Process.Start("/bin/bash", "-c \"sudo systemctl stop apptask-api.service\"");

            process.WaitForExit();
        }
    }
}
