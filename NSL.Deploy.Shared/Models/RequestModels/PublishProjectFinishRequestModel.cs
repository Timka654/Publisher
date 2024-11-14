using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System.Collections.Generic;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class PublishProjectFinishRequestModel
    {
        public Dictionary<string,string> Args { get; set; }
    }
}
