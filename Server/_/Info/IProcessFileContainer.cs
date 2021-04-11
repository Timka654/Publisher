using Publisher.Server.Info;

namespace Publisher.Server._.Info
{
    interface IProcessFileContainer
    {
        ProjectFileInfo CurrentFile { get; set; }
    }
}
