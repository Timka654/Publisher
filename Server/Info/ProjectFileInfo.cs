using Publisher.Basic;
using System;
using System.IO;
using Utils;

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
            var fi = new FileInfo(System.IO.Path.Combine(Project.TempDirPath, RelativePath));

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            IO = fi.Create();
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

            CalculateHash();

            Project.BroadcastMessage($"{FileInfo.Name} new hash -> {Hash}");

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
