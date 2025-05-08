namespace ActiveSense.Desktop.Infrastructure.Process.Interfaces;

public interface IFileManager
{
    void CopyFiles(string[] files, string processingDirectory, string outputDirectory, string[] supportedFileTypes);
}