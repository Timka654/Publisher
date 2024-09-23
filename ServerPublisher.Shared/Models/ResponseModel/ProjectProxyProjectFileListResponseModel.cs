using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Info;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType("Default")]
    public partial class ProjectProxyProjectFileListResponseModel
    {
        [NSLBIOInclude("Default"), NSLBIOProxy("Get", "Default")]
        public DownloadFileInfo[] FileList { get; set; }
    }
}
