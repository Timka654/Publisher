using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyUpdateDataRequestModel
    {
        public string ProjectId { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
