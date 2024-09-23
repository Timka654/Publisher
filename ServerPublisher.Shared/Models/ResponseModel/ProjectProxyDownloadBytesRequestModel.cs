using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyDownloadBytesRequestModel
    {
        public int BufferLength { get; set; }
    }
}
