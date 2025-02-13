using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ServerPublisher.Shared.Info
{
    public class UploadFileInfo : BasicFileInfo
    {
        public string OutputRelativePath { get; set; }

        public UploadFileInfo(string outputRelativePath, string relativePath, string dir, FileInfo fi) : base(dir, fi)
        {
            if (!string.IsNullOrWhiteSpace(outputRelativePath))
                OutputRelativePath = Path.Combine(outputRelativePath, relativePath).GetNormalizedPath();
            else
                OutputRelativePath = relativePath;
        }

        public UploadFileInfo(string outputRelativePath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(outputRelativePath))
            {
                OutputRelativePath = relativePath;
                return;
            }

            OutputRelativePath = Path.Combine(outputRelativePath, relativePath);
        }
    }

    public class BasicFileInfo
    {
        [NSLBIOInclude("Get")]
        public virtual string RelativePath { get; set; }

        [NSLBIOInclude("Get")]
        public virtual string Hash { get; set; }

        [NSLBIOInclude("Get")]
        public virtual DateTime LastChanged { get; set; }

        public FileInfo FileInfo { get; set; }

        public BasicFileInfo(string dir, FileInfo finfo)
        {
            FileInfo = finfo;

            RelativePath = finfo.GetNormalizedRelativePath(dir);
        }

        public void CalculateHash()
        {
            if (FileInfo.Exists == false)
            {
                Hash = "";
                return;
            }

            using var fs = File.OpenRead(FileInfo.GetNormalizedFilePath());

            Hash = string.Join("", MD5.HashData(fs).Select(x => x.ToString("X2")));
        }

        public BasicFileInfo()
        { }


    }
}
