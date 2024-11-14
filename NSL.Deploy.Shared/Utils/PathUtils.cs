using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPublisher.Shared.Utils
{
    public static class PathUtils
    {
        public static string GetNormalizedPath(this string path)
            => path.Replace('\\', '/');

        public static string GetNormalizedFilePath(this FileInfo file)
            => file.FullName.Replace('\\', '/');

        public static string GetNormalizedDirectoryPath(this DirectoryInfo directory)
            => directory.FullName.Replace('\\', '/');

        public static string GetNormalizedRelativePath(this FileInfo file, string directory)
            => Path.GetRelativePath(directory.GetNormalizedPath(), file.GetNormalizedFilePath()).GetNormalizedPath();
        public static string GetNormalizedRelativePath(this FileInfo file, DirectoryInfo directory)
            => Path.GetRelativePath(directory.GetNormalizedDirectoryPath(), file.GetNormalizedFilePath()).GetNormalizedPath();
    }
}
