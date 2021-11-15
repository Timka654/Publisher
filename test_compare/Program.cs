
using System.Security.Cryptography;

var srcDir = @"D:\Projects\AppFox\CRM\SeoEmailTracker\SeoEmailTracker\publish";

var srcDirInfo = new DirectoryInfo(srcDir);

var destDir = @"D:\Temp\12\test\MainHost";

var destDirInfo = new DirectoryInfo(destDir);

var md5 = MD5.Create();

foreach (var item in srcDirInfo.GetFiles("*", SearchOption.AllDirectories))
{
    var rel = Path.GetRelativePath(srcDir, item.FullName);

    var destPath = Path.Combine(destDir, rel);
    if (File.Exists(destPath) == false)
        Console.WriteLine($"dest file not found {rel}");
    else if (md5.ComputeHash(File.ReadAllBytes(item.FullName)).SequenceEqual(md5.ComputeHash(File.ReadAllBytes(destPath))) == false)
        Console.WriteLine($"dest file error hash {rel}");
}

foreach (var item in destDirInfo.GetFiles("*", SearchOption.AllDirectories))
{
    var rel = Path.GetRelativePath(destDir, item.FullName);

    var destPath = Path.Combine(srcDir, rel);
    if (File.Exists(destPath) == false)
        Console.WriteLine($"src file not found {rel}");
    else if (md5.ComputeHash(File.ReadAllBytes(item.FullName)).SequenceEqual(md5.ComputeHash(File.ReadAllBytes(destPath))) == false)
        Console.WriteLine($"src file error hash {rel}");
}