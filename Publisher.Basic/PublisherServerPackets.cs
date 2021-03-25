namespace Publisher.Basic
{
    public enum PublisherServerPackets : ushort
    {
        SignIn = 1,
        ProjectFileList,
        
        ProjectPublishStart,
        ProjectPublishEnd,

        FilePublishStart = 6,
        UploadFileBytes,
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
