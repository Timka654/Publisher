using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class PublishProjectUploadFileBytesRequestModel
    {
        public Guid FileId { get; set; }

        public long Offset { get; set; }

        public byte[]? Bytes { get; set; }
    }
}
