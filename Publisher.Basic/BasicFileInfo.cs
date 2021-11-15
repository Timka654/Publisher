using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Publisher.Basic
{
    public class DownloadFileInfo : BasicFileInfo
    {
        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }

    public class BasicFileInfo
    {
        public virtual string RelativePath { get; set; }

        public virtual string Hash { get; set; }

        public virtual DateTime LastChanged { get; set; }

        public FileInfo FileInfo { get; set; }

        public BasicFileInfo(string dir, System.IO.FileInfo finfo)
        {
            FileInfo = finfo;

            RelativePath = Path.GetRelativePath(dir, finfo.FullName);
        }

        public void CalculateHash()
        {
            if (FileInfo.Exists == false)
            {
                Hash = "";
                return;
            }

            using MD5 md5 = MD5.Create();

            Hash = string.Join("", md5.ComputeHash(File.ReadAllBytes(FileInfo.FullName)).Select(x => x.ToString("X2")));
        }

        public BasicFileInfo()
        { }


    }
}
