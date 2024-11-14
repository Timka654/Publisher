using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class ProjectProxySignInRequestModel
    {
        public string UserId { get; set; }

        public string ProjectId { get; set; }

        public byte[] IdentityKey { get; set; }

        public DateTime LatestUpdate { get; set; }
    }
}
