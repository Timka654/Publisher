﻿using ServerPublisher.Shared.Info;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;

namespace ServerPublisher.Server.Info
{
    public class ProjectFileInfo : BasicFileInfo
    {
        public string Path => FileInfo.GetNormalizedFilePath();

        public override DateTime LastChanged => FileInfo.LastWriteTimeUtc;

        public FileStream WriteIO { get; set; }

        public ServerProjectInfo Project { get; set; }

        public ProjectFileInfo(string dir, FileInfo finfo, ServerProjectInfo project) : base(dir, finfo)
        {
            Project = project;
        }

        private DateTime? createTime;
        private DateTime? updateTime;
        private FileInfo fi;

        public void StartFile(ProjectPublishContext context, DateTime createTime, DateTime updateTime)
        {
            fi = new FileInfo(System.IO.Path.Combine(context.TempPath, RelativePath).GetNormalizedPath());

            if (fi.Directory.Exists == false)
                fi.Directory.Create();

            WriteIO = fi.Create();

            this.createTime = createTime;
            this.updateTime = updateTime;

            Project.BroadcastMessage($"starting -> {RelativePath}");
        }

        public bool EndFile()
        {
            if (WriteIO == null)
                return false;
            WriteIO.Flush();
            WriteIO.Dispose();
            WriteIO = null;
            try { fi.CreationTimeUtc = createTime.Value; } catch (Exception ex) { Project.BroadcastMessage($"finishing error -> {ex}"); }
            createTime = null;
            try { fi.LastWriteTimeUtc = updateTime.Value; } catch (Exception ex) { Project.BroadcastMessage($"finishing error -> {ex}"); }
            updateTime = null;

            fi = null;

            Project.BroadcastMessage($"uploaded -> {RelativePath}");

            return true;
        }

        public bool TempRelease(IProcessingFilesContext context)
        {
            var fi = new FileInfo(System.IO.Path.Combine(context.TempPath, RelativePath).GetNormalizedPath());

            if (fi.Exists == false)
            {
                Project.BroadcastMessage($"Error!! {fi.GetNormalizedFilePath()} not exists!!");
                return false;
            }

            if (FileInfo.Directory.Exists == false)
                FileInfo.Directory.Create();

            fi.MoveTo(FileInfo.GetNormalizedFilePath(), true);

            if (!FileInfo.Exists)
                FileInfo = new FileInfo(FileInfo.GetNormalizedFilePath());

            CalculateHash();

            Project.BroadcastMessage($"{RelativePath} new hash -> {Hash}");

            return true;
        }

        public Stream OpenRead()
            => FileInfo.OpenRead();

        public ProjectFileInfo()
        {

        }
    }
}
