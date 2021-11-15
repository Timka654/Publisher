using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Utils
{
    public static void CmdExec(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        Process cmdProc = default;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            cmdProc = Process.Start("cmd", $"-c \"{command}\"");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            cmdProc = Process.Start("/bin/bash", $"-c \"{command}\"");

        if (cmdProc == null)
            throw new NotImplementedException($"CmdExec: not have cmd command for currentPlatform");

        cmdProc.WaitForExit();
    }
}