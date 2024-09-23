using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Info;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType("Default")]
    public partial class ProjectFileListResponseModel
    {
        [NSLBIOInclude("Default"), NSLBIOProxy("Get", "Default")]
        public BasicFileInfo[] FileList { get; set; }
    }
}
