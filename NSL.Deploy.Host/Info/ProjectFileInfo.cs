using ServerPublisher.Server.Scripts;
using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Info
{
    public class ProjectFileInfo : BasicFileInfo, IScriptableFileInfo
    {
        public string Path => FileInfo.GetNormalizedFilePath();

        public override DateTime LastChanged => FileInfo.LastWriteTimeUtc;

        private FileStream? WriteIO { get; set; }

        public ServerProjectInfo Project { get; set; }

        public ProjectFileInfo(string dir, FileInfo finfo, ServerProjectInfo project) : base(dir, finfo)
        {
            Project = project;
        }

        private DateTime? createTime;
        private DateTime? updateTime;
        private FileInfo fi;

        private SemaphoreSlim? IOLocker;

        public void StartFile(ProjectPublishContext context, DateTime createTime, DateTime updateTime, string hash)
        {
            fi = new FileInfo(System.IO.Path.Combine(context.TempPath, RelativePath).GetNormalizedPath());

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            IOLocker = new SemaphoreSlim(1);
            WriteIO = fi.Create();

            this.createTime = createTime;
            this.updateTime = updateTime;
            this.Hash = hash;

            context.Log($"starting -> {RelativePath}");
        }

        public bool EndFile(ProjectPublishContext context)
        {
            if (WriteIO == null)
                return false;

            WriteIO.Flush();
            WriteIO.Dispose();
            WriteIO = null;

            try { fi.CreationTimeUtc = createTime.Value; } catch (Exception ex) { context.Log($"finishing error -> {ex}"); }
            createTime = null;

            try { fi.LastWriteTimeUtc = updateTime.Value; } catch (Exception ex) { context.Log($"finishing error -> {ex}"); }
            updateTime = null;

            fi = null;
            IOLocker?.Dispose();
            IOLocker = null;

            context?.Log($"uploaded -> {RelativePath}");

            return true;
        }

        public async Task WriteAsync(Stream fromStream)
        {
            if (WriteIO == null)
                return;

            await IOLocker.WaitAsync();

            fromStream.CopyTo(WriteIO);

            IOLocker.Release();
        }

        public async Task WriteAsync(byte[] fromStream, long offset, Func<Task>? threadSafeAction = null)
        {
            if (WriteIO == null)
                return;

            await IOLocker.WaitAsync();

            WriteIO.Position = offset;
            await WriteIO.WriteAsync(fromStream);

            if (threadSafeAction != null)
                await threadSafeAction();

            IOLocker.Release();
        }

        public void ReleaseIO()
        {
            if (WriteIO == null)
                return;

            WriteIO.Flush();
            WriteIO.Dispose();
            WriteIO = null;
            IOLocker?.Dispose();
            IOLocker = null;
        }

        public void RemoveFile()
        {
            FileInfo.Delete();
        }


        public bool TempRelease(IProcessingFilesContext context, bool checkHash)
        {
            var fi = new FileInfo(System.IO.Path.Combine(context.TempPath, RelativePath).GetNormalizedPath());

            if (fi.Exists == false)
            {
                context.Log($"Error!! {fi.GetNormalizedFilePath()} not exists!!");

                return false;
            }

            if (FileInfo.Directory.Exists == false)
                FileInfo.Directory.Create();

            fi.MoveTo(FileInfo.GetNormalizedFilePath(), true);

            if (!FileInfo.Exists)
                FileInfo = new FileInfo(FileInfo.GetNormalizedFilePath());

            var oldHash = Hash;
            
            CalculateHash();

            if (checkHash && oldHash != Hash)
            {
                context.Log($"{RelativePath} hash not match -> {oldHash} != {Hash}");
                return false;
            }

            context.Log($"{RelativePath} new hash -> {Hash}");

            return true;
        }

        public Stream OpenRead()
            => FileInfo.OpenRead();

        public ProjectFileInfo()
        {

        }
    }
}
