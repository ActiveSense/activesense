using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ActiveSense.Desktop.Export.Interfaces;

namespace ActiveSense.Desktop.Export.Implementations;

public class ArchiveCreator : IArchiveCreator
{
    public async Task<bool> CreateArchiveAsync(string outputPath, string pdfPath,
        string fileName, string sleepCsv, string activityCsv)
    {
        try
        {
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // Add PDF
                await AddFileToArchiveAsync(archive, $"{fileName}_report.pdf", pdfPath);

                // Add sleep CSV
                await AddTextToArchiveAsync(archive, $"{fileName}_sleep.csv", sleepCsv);

                // Add activity CSV
                await AddTextToArchiveAsync(archive, $"{fileName}_activity.csv", activityCsv);
            }

            zipStream.Seek(0, SeekOrigin.Begin);
            await using var fileStream = new FileStream(outputPath, FileMode.Create);
            await zipStream.CopyToAsync(fileStream);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating archive: {ex.Message}");
            return false;
        }
    }

    private async Task AddFileToArchiveAsync(ZipArchive archive, string entryName, string filePath)
    {
        var entry = archive.CreateEntry(entryName);
        await using var entryStream = entry.Open();
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(entryStream);
    }

    private async Task AddTextToArchiveAsync(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        await using var entryStream = entry.Open();
        await using var writer = new StreamWriter(entryStream);
        await writer.WriteAsync(content);
    }
}