using ServerPublisher.Shared;
using System;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public class ProjectFileInfo : BasicFileInfo
    {
        public string Path => FileInfo.FullName;

        public override DateTime LastChanged => FileInfo.LastWriteTimeUtc;

        public FileStream IO { get; set; }

        public ServerProjectInfo Project { get; set; }

        public ProjectFileInfo(string dir, FileInfo finfo, ServerProjectInfo project) : base(dir, finfo)
        {
            Project = project;
        }

        private DateTime? createTime;
        private DateTime? updateTime;
        private FileInfo fi;

        public void StartFile(DateTime createTime, DateTime updateTime)
        {
            fi = new FileInfo(System.IO.Path.Combine(Project.TempDirPath, RelativePath));

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            IO = fi.Create();

            this.createTime = createTime;
            this.updateTime = updateTime;

            Project.BroadcastMessage($"starting -> {RelativePath}");
        }

        public bool EndFile()
        {
            if (IO == null)
                return false;
            IO.Flush();
            IO.Dispose();
            IO = null;
            try { fi.CreationTimeUtc = createTime.Value; } catch (Exception ex) { Project.BroadcastMessage($"starting error -> {ex}"); }
            createTime = null;
            try { fi.LastWriteTimeUtc = updateTime.Value; } catch (Exception ex) { Project.BroadcastMessage($"starting error -> {ex}"); }
            updateTime = null;

            fi = null;

            Project.BroadcastMessage($"uploaded -> {RelativePath}");

            return true;
        }

        public bool TempRelease()
        {
            var fi = new FileInfo(System.IO.Path.Combine(Project.TempDirPath, RelativePath));

            if (fi.Exists == false)
            {
                Project.BroadcastMessage($"Error!! {fi.FullName} not exists!!");
                return false;
            }

            if (FileInfo.Directory.Exists == false)
                FileInfo.Directory.Create();

            fi.MoveTo(FileInfo.FullName, true);

            if (!FileInfo.Exists)
                FileInfo = new FileInfo(FileInfo.FullName);

            CalculateHash();

            Project.BroadcastMessage($"{RelativePath} new hash -> {Hash}");

            return true;
        }

        public void OpenRead()
        {
            IO = FileInfo.OpenRead();
        }

        public void CloseRead()
        {
            IO?.Dispose();
            IO = null;
        }

        public ProjectFileInfo()
        {

        }
    }
}
