﻿using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using ServerPublisher.Shared.Utils;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ServerPublisher.Shared.Info
{
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

            using MD5 md5 = MD5.Create();

            Hash = string.Join("", md5.ComputeHash(File.ReadAllBytes(FileInfo.GetNormalizedFilePath())).Select(x => x.ToString("X2")));
        }

        public BasicFileInfo()
        { }


    }
}
