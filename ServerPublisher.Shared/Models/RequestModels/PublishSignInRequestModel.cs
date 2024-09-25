using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class PublishSignInRequestModel
    {
        public string UserId { get; set; }

        public string ProjectId { get; set; }

        public byte[] IdentityKey { get; set; }

        public OSTypeEnum OSType { get; set; }
    }
}
