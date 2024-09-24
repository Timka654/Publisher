namespace ServerPublisher.Shared.Enums
{
    public enum PublisherPacketEnum : ushort
    {
        Response = 1,

        ServerLog,

        PublishProjectSignIn,
        PublishProjectFileList,
        PublishProjectFinish,
        PublishProjectFileStart,
        PublishProjectUploadFilePart,



        ProjectProxyDownloadBytes,
        ProjectProxyFinishDownload,
        ProjectProxyProjectFileList,
        ProjectProxySignIn,
        ProjectProxySignOut,
        ProjectProxyStartDownload,
        ProjectProxyUpdateDataMessage,










        //ProjectPublishStart,
        //PublisherFileList,

        //DownloadBytes,
        //NextFile,
        //FinishDownload,
        //StartDownload,
        //SignOut,

        //PatchSignIn,

        //ChangeLatestUpdateHandle,

        ExplorerCreateSignFile,
        ExplorerDownloadFile,
        ExplorerGetFileList,
        ExplorerGetProjectList,
        ExplorerPathRemove,
        ExplorerRemoveSignFile,
        ExplorerSignIn,
        ExplorerUploadFile
    }
}
