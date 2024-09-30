using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyStartFileResponseModel
    {
        public bool Result { get; set; }

        public Guid FileId { get; set; }
    }
}
