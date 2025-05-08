using System;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Process;

public class FileManager(IPathService pathService) : IFileManager
{
    public void CopyFiles(string[] files, string processingDirectory, string outputDirectory, string[] supportedFileTypes)
    {
        pathService.ClearDirectory(processingDirectory);
        pathService.EnsureDirectoryExists(outputDirectory);
        
        foreach (var file in files)
            try
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                var fileName = Path.GetFileName(file);

                if (supportedFileTypes.Contains(extension))
                {
                    var destinationPath = Path.Combine(processingDirectory, fileName);
                    File.Copy(file, destinationPath, true);
                }
                else if (extension == ".pdf")
                {
                    var destinationPath = Path.Combine(outputDirectory, fileName);
                    File.Copy(file, destinationPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file {file}: {ex.Message}");
            }
    }
}