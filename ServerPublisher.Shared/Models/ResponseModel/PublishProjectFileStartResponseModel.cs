using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class PublishProjectFileStartResponseModel
    {
        [NSLBIOInclude] public bool Result { get; set; }

        [NSLBIOInclude] public Guid FileId { get; set; }
    }
}
