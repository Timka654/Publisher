using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class PublishProjectFileStartRequestModel
    {
        public string RelativePath { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
