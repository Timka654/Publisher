﻿using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxyFileListRequestModel
    {
        public string ProjectId { get; set; }
    }
}
