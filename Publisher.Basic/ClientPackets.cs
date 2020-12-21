namespace Publisher.Basic
{
    public enum ClientPackets
    {
        SignInResult = 1,
        FileListResult,

        ServerLog,
        ProjectPublishStart = 5,
        ProjectPublishEndResult,
        FilePublishStartResult,
        UploadFileBytesResult,
        ExplorerDownloadFileResult,
        ExplorerCreateSignFileResult,
        ExplorerSignInResult,
        ExplorerRemoveSignFileResult,
        ExplorerPathRemoveResult,
        ExplorerGetProjectListResult,
        ExplorerGetFileListResult,
        ExplorerUploadFileResult
    }
}
