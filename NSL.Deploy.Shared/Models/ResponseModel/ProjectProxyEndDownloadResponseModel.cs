using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace ServerPublisher.Shared.Models.ResponseModel
{
    [NSLBIOType]
    public partial class ProjectProxyEndDownloadResponseModel
    {
        public bool Success { get; set; }

        public FileDownloadDataModel[] FileList { get; set; }
    }
}
