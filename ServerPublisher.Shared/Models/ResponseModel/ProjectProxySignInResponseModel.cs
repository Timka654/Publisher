using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Enums;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxySignInResponseModel
    {
        public SignStateEnum Result { get; set; }
    }
}
