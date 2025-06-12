using System;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using Serilog;

namespace ActiveSense.Desktop.Infrastructure.Process.Helpers;

public class FileManager(IPathService pathService, ILogger logger) : IFileManager
{
    public void CopyFiles(string[] files, string processingDirectory, string outputDirectory,
        string[] supportedFileTypes)
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
                    logger.Information("Copying file {File} to {Destination}", file, destinationPath);
                    File.Copy(file, destinationPath, true);
                }
                else if (extension == ".pdf")
                {
                    logger.Information("Copying PDF file {File} to {Destination}", file, outputDirectory);
                    var destinationPath = Path.Combine(outputDirectory, fileName);
                    File.Copy(file, destinationPath, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Kopieren des Files {file}." + ex.Message);
            }

        logger.Information("All files copied to {Directory}", processingDirectory);
    }
}