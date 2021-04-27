using System;
using System.Diagnostics;
using System.IO;

namespace debug_console
{
    class Program
    {
        static void Main(string[] args)
        {
            Process p = Process.Start("/bin/bash", $"-c \"chmod +x '{Path.Combine(ScriptCore.Instance.GlobalData.CurrentProject.ProjectDirPath, "CRM.Service.TaskService")}'\"");
            p.WaitForExit();
            Process p = Process.Start("/bin/bash", $"-c \"systemctl start publisher.service\"");
            p.WaitForExit();



            var process = System.Diagnostics.Process.Start("/bin/bash", "-c \"sudo systemctl stop apptask-api.service\"");

            process.WaitForExit();
        }
    }
}
