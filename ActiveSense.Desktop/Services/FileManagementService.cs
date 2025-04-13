using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ActiveSense.Desktop.Services;

public static class FileService
{
    public static async Task<bool> CopyFilesToDirectoryAsync(
        IEnumerable<string> sourcePaths, string destinationDirectory)
    {
        try
        {
            Directory.CreateDirectory(destinationDirectory);
            
            foreach (var sourcePath in sourcePaths)
            {
                var fileName = Path.GetFileName(sourcePath);
                string destinationPath = Path.Combine(destinationDirectory, fileName);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                await Task.Run(() => File.Copy(sourcePath, destinationPath));
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Could not copy files to {destinationDirectory}", e);
        }
    }

    public static IEnumerable<string> GetFilesInDirectory(string searchDirectory, string searchPattern)
    {
        if (!Directory.Exists(searchDirectory))
        {
            Console.WriteLine($"Directory does not exist: {searchDirectory}");
            return Array.Empty<string>();
        }

        return Directory.GetFiles(searchDirectory, searchPattern);
    }
}