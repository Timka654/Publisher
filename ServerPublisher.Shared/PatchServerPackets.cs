namespace ServerPublisher.Shared
{
    public enum PatchServerPackets : ushort
    {
        SignIn = 100,
        ProjectFileList,

        PublisherFileList,

        DownloadBytes,
        NextFile,
        FinishDownload,
        StartDownload,
        SignOut,
    }
}
