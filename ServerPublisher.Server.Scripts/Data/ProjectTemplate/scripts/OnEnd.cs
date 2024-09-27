public partial class PublisherScript {
	public static void OnEnd(IScriptableServerProjectInfo project, bool success, bool postProcessingSuccess, Dictionary<string, string> args) {
		if (success && postProcessingSuccess)
		{
            //var execPath = Path.Combine(ScriptCore.ProjectInfo.ProjectDirPath,"testapp");

            //Utils.BashExec($"sudo chmod +x '{execPath}'");

            //Utils.BashExec("sudo systemctl start test.service");
        }
    }
}
