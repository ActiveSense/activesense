using System;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Process.Interfaces;

namespace ActiveSense.Desktop.Process.Implementations;

public class FileManager : IFileManager
{
    public void CopyFiles(string[] files, string processingDirectory, string outputDirectory, string[] supportedFileTypes)
    {
        ClearDirectory(processingDirectory);
        Directory.CreateDirectory(processingDirectory);
        Directory.CreateDirectory(outputDirectory);

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

    public void ClearDirectory(string directory)
    {
        if (Directory.Exists(directory))
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing directory: {ex.Message}");
            }
    }
}