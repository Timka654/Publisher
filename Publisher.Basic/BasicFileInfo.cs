using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Basic
{
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
            using MD5 md5 = MD5.Create();

            Hash = string.Join("", md5.ComputeHash(File.ReadAllBytes(FileInfo.FullName)).Select(x => x.ToString("X2")));
        }

        public BasicFileInfo()
        { }


    }
}
