
namespace ServerPublisher.Server.Scripts
{
    public interface IScriptableServerProjectInfo
    {
        string ProjectDirPath { get; }

        bool IsLinux();

        bool IsWindows();

        bool IsMacOS();
    }
}
