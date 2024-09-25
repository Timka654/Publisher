namespace ServerPublisher.Shared.Enums
{
    public enum PublisherPacketEnum : ushort
    {
        Response = 1,

        ServerLog,

        PublishProjectSignIn,
        PublishProjectFinish,
        PublishProjectFileStart,
        PublishProjectUploadFilePart,
        PublishProjectPublishStartMessage,



        ProjectProxySignIn,
        ProjectProxySignOut,
        ProjectProxyUpdateDataMessage,
        ProjectProxyStartFile,
        ProjectProxyDownloadBytes,
        ProjectProxyFinishFile,
        ProjectProxyStartDownload,
        ProjectProxyProjectFileList,
        ProjectProxyFinishDownload,










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
