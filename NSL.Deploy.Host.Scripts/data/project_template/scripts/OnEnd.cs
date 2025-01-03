public partial class PublisherScript
{
    public static void OnEnd(ScriptInvokingContext context, bool success, bool postProcessingSuccess, Dictionary<string, string> args)
    {
        //if (success && postProcessingSuccess)
        //{
        //    //var execPath = Path.Combine(ScriptCore.ProjectInfo.ProjectDirPath,"testapp");

        //    //Utils.CmdExec($"sudo chmod +x '{execPath}'");

        //    //Utils.CmdExec("sudo systemctl start test.service");
        //}
        //if (success && postProcessingSuccess && context.Executor.AnyFiles())
        //{

        //    var dockerFilesPath = System.IO.Path.Combine(context.Project.ProjectDirPath, "LocalDockerDeploy");

        //    if (System.IO.Directory.Exists(dockerFilesPath))
        //    {
        //        foreach (var file in System.IO.Directory.GetFiles(dockerFilesPath))
        //        {
        //            System.IO.File.Copy(file, System.IO.Path.Combine(context.Project.ProjectDirPath, System.IO.Path.GetRelativePath//    (dockerFilesPath, file)), true);
        //        }
        //    }

        //    // create/upload docker image
        //    Utils.CmdExec(context, [
        //        $"cd \"{context.Project.ProjectDirPath}\"",
        //        "docker build -t dotnet_instance -f Dockerfile .",
        //        "docker tag dotnet_instance:latest localhost:5000/dotnet_instance",
        //        "docker push localhost:5000/dotnet_instance"]);

        //    // reboot instance with update
        //    Utils.CmdExec(context, [
        //        "cd \"/root/dotnet_instance\"",
        //        "docker compose pull",
        //        "docker compose down",
        //        "sleep 2",
        //        "docker compose up --force-recreate --build -d",
        //        "docker image prune -f"
        //    ]);
        //}
    }
}
