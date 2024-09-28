using System.IO;

namespace ServerPublisher.Server.Dev.Test.Utils
{
    public static class DirectoryUtils
    {
        public static void CreateNoExistsDirectory(this DirectoryInfo di)
        {
            CreateNoExistsDirectory(di.FullName);
        }

        public static void CreateNoExistsDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
