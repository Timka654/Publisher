using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal class Utils
{
    public static void CmdExec(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        Process cmdProc = Process.Start("cmd", $"-c \"{command}\"");

        cmdProc.WaitForExit();
    }

    public static void BashExec(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        Process cmdProc = Process.Start("/bin/bash", $"-c \"{command}\"");

        cmdProc.WaitForExit();
    }
}