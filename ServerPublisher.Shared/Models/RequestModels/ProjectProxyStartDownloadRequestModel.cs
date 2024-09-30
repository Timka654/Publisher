using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyStartDownloadRequestModel
    {
        public string ProjectId { get; set; }

        public TransportModeEnum TransportMode { get; set; }
    }
}
