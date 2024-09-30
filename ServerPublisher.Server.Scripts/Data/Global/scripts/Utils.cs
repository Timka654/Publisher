internal class Utils
{
    public static void CmdExec(ScriptInvokingContext context, string[] commands)
    {
        if (!commands.Any())
            return;

        var cmd = context.Project.IsWindows() ? "cmd" : "/bin/bash";

        var si = new ProcessStartInfo(cmd)
        {
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
        };

        Process cmdProc = Process.Start(si);

        cmdProc.ErrorDataReceived += (s, e) => context.Executor.Log(e.Data);
        cmdProc.OutputDataReceived += (s, e) => context.Executor.Log(e.Data);
        cmdProc.EnableRaisingEvents = true;

        cmdProc.Start();

        cmdProc.BeginOutputReadLine();
        cmdProc.BeginErrorReadLine();

        var input = cmdProc.StandardInput;

        foreach (var item in commands)
        {
            input.WriteLine(item);
            input.Flush();
        }

        if (commands.Last() != "exit")
        {
            input.WriteLine("exit");
            input.Flush();
        }

        cmdProc.WaitForExit();
    }
}