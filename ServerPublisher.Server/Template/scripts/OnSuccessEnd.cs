using System.IO;

public partial class PublisherScript {
	public static void OnSuccessEnd(Dictionary<string, object> args) {

		//Utils.BashExec($"sudo chmod +x '{Path.Combine(ScriptCore.Instance.GlobalData.CurrentProject.ProjectDirPath,"testapp")}'");

		//Utils.BashExec("sudo systemctl start test.service");
	}
}
