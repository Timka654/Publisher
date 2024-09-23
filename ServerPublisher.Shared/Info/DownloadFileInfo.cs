using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Info
{
    public class DownloadFileInfo : BasicFileInfo
    {
        [NSLBIOInclude("Get")]
        public DateTime CreationTime { get; set; }

        [NSLBIOInclude("Get")]
        public DateTime ModifiedTime { get; set; }
    }
}
