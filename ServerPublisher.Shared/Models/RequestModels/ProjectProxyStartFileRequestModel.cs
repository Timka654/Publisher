using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyStartFileRequestModel
    {
        public string ProjectId { get; set; }

        public string RelativePath { get; set; }
    }
}
