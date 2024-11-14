using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyProjectProxyFinishFileRequestModel
    {
        public Guid FileId { get; set; }
    }
}
