using QuickUpdater;
using System.IO.Compression;

using var httpClient = new HttpClient();

var response = await httpClient.GetAsync(Resource.update_archive_url);

using (ZipArchive za = new ZipArchive(response.Content.ReadAsStream()))
{
    foreach (var item in za.Entries)
    {
        item.ExtractToFile(item.FullName, true);
    }
}