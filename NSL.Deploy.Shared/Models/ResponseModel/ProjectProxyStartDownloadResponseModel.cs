using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System.Collections.Generic;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyStartDownloadResponseModel
    {
        public bool Result { get; set; }

        public List<string> IgnoreFilePathes { get; set; }
    }
}
