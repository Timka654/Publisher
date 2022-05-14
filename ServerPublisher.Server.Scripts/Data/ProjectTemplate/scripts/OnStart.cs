using ServerPublisher.Server.Scripts;

public partial class PublisherScript {
	public static void OnStart(IScriptableServerProjectInfo project) {
		Utils.BashExec("sudo systemctl stop test.service");
	}
}
