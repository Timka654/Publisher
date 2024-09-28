

namespace ServerPublisher.Server.Scripts
{
    public interface IScriptableFileInfo
    {
        string Path { get; }
        DateTime LastChanged { get; }
    }
}
