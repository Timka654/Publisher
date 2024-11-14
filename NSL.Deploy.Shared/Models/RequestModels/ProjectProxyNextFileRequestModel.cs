using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyNextFileRequestModel
    {
        public string Path { get; set; }
    }
}
