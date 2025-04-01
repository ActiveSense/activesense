using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace ActiveSense.Desktop.Services;

public interface IFileService
{
    Task<bool> CopyFilesToDirectoryAsync(IEnumerable<string> sourcePaths, string targetDirectory);
    IEnumerable<string> GetFilesInDirectory(string directory, string searchPattern);
}

public class FileService : IFileService
{
    public async Task<bool> CopyFilesToDirectoryAsync(
        IEnumerable<string> sourcePaths, string targetDirectory)
    {
        try
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (var sourcePath in sourcePaths)
            {
                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(targetDirectory, fileName);

                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                await Task.Run(() => File.Copy(sourcePath, destPath));
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public IEnumerable<string> GetFilesInDirectory(string directory, string searchPattern)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Directory does not exist: {directory}");
            return Array.Empty<string>();
        }

        return Directory.GetFiles(directory, searchPattern);
    }
}
