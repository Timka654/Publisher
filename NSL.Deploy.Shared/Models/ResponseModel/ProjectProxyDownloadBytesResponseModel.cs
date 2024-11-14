using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyDownloadBytesResponseModel
    {
        public byte[] Bytes { get; set; }

        public bool EOF { get; set; }
    }
}
