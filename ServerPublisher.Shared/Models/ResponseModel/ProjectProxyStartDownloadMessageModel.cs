using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Info;
using System;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType("Default")]
    public partial class ProjectProxyStartDownloadMessageModel
    {
        [NSLBIOInclude("Default")]
        public string ProjectId { get; set; }

        [NSLBIOInclude("Default"), NSLBIOProxy("Get", "Default")]
        public DownloadFileInfo[] FileList { get; set; }
    }
}
