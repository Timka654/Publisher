using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyDownloadBytesRequestModel
    {
        public string RelativePath { get; set; }

        public int BufferLength { get; set; }
    }
}
