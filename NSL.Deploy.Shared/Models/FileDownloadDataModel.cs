namespace ServerPublisher.Shared.Models
{
    public class FileDownloadDataModel
    {
        public string RelativePath { get; set; }

        public byte[] Data { get; set; }
    }
}
