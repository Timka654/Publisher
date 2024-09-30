
namespace ServerPublisher.Server.Scripts
{
    public interface IScriptableExecutorContext
    {
        void Log(string content, bool appLog = false);

        bool AnyFiles();
    }
}
