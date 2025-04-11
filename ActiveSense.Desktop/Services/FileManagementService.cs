using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ActiveSense.Desktop.Services;

public interface IFileService
{
    Task<bool> CopyFilesToDirectoryAsync(IEnumerable<string> sourcePaths, string processingDirectory, string outputsDirectory);
    IEnumerable<string> GetFilesInDirectory(string directory, string searchPattern);
}

public class FileService : IFileService
{
    private readonly string[] _extensionsToExclude = [".csv"]; //exclude from processing
    private readonly string[] _extensionsToInclude = [".bin"];

    public async Task<bool> CopyFilesToDirectoryAsync(
        IEnumerable<string> sourcePaths, string processingDirectory, string outputsDirectory)
    {
        try
        {
            Directory.CreateDirectory(processingDirectory);
            Directory.CreateDirectory(outputsDirectory);
            
            foreach (var sourcePath in sourcePaths)
            {
                var fileName = Path.GetFileName(sourcePath);
                var fileExtension = Path.GetExtension(sourcePath);
                string destinationPath;

                if (_extensionsToExclude.Contains(fileExtension))
                {
                    destinationPath = Path.Combine(outputsDirectory, fileName);
                    Console.WriteLine($"Copying {fileName} to {outputsDirectory}");
                }
                else if (_extensionsToInclude.Contains(fileExtension))
                {
                    destinationPath = Path.Combine(processingDirectory, fileName);
                    Console.WriteLine($"Copying {fileName} to {processingDirectory}");
                }
                else
                {
                    destinationPath = Path.Combine(processingDirectory, fileName);
                    Console.WriteLine();
                }

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
            throw new Exception($"Could not copy files from {processingDirectory} to {outputsDirectory}", e);
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