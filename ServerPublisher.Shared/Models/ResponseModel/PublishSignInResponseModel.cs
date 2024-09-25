using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Enums;
using System.Collections.Generic;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class PublishSignInResponseModel
    {
        [NSLBIOInclude] public SignStateEnum Result { get; set; }

        [NSLBIOInclude] public List<string> IgnoreFilePatterns { get; set; }
    }
}
