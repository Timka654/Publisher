using Publisher.Basic;
using System;
using System.IO;

namespace Publisher.Server.Info
{
    public class ProjectFileInfo : BasicFileInfo
    {
        public string Path => FileInfo.FullName;

        public override DateTime LastChanged => FileInfo.LastWriteTimeUtc;

        public FileStream IO { get; set; }

        public ProjectInfo Project { get; set; }

        public ProjectFileInfo(string dir, System.IO.FileInfo finfo, ProjectInfo project) : base(dir,finfo)
        {
            Project = project;
        }

        public void StartFile()
        {
            if (!FileInfo.Directory.Exists)
                FileInfo.Directory.Create();
            IO = FileInfo.Create();
            Project.BroadcastMessage($"starting -> {RelativePath}");
        }

        public bool EndFile()
        {
            if (IO == null)
                return false;
            IO.Flush();
            IO.Dispose();
            IO = null;
            Project.BroadcastMessage($"uploaded -> {RelativePath}");
            CalculateHash();

            Project.BroadcastMessage($"new hash -> {Hash}");
            return true;
        }

        public void OpenRead()
        {
            IO = FileInfo.OpenRead();
        }
        public void CloseRead()
        {
            IO.Dispose();
            IO = null;
        }

        public ProjectFileInfo()
        { 
        
        }
    }
}
