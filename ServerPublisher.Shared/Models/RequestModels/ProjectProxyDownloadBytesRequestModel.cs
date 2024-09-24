using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyDownloadBytesRequestModel
    {
        public Guid FileId { get; set; }

        public int BufferLength { get; set; }
    }
}
