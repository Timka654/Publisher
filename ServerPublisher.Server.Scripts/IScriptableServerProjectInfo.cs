
namespace ServerPublisher.Server.Scripts
{
    public interface IScriptableServerProjectInfo
    {
        string ProjectDirPath { get; }

        void BroadcastMessage(string log);
    }
}
