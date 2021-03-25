namespace Publisher.Basic
{
    public enum PatchClientPackets : ushort
    {
        SignInResult = 100,

        ProjectFileListResult,

        DownloadBytesResult,
        StartDownloadResult,
        ChangeLatestUpdateHandle,
        FinishDownloadResult,
    }
}
