using System.IO;
using ActiveSense.Desktop.Services;
using NUnit.Framework;
using FluentAvalonia.Core;
using Assert = NUnit.Framework.Assert;

namespace ActiveSense.Desktop.Tests.Tests;

[TestFixture]
public class FileManagementTests
{

    [Test]
    public void CopyFilesToDirectoryMultipleTypes()
    {
        var sourceFiles = FileService.GetFilesInDirectory(AppConfig.InputDirectoryPath, "*.*");
        var destination = Path.Combine(AppConfig.OutputsDirectoryPath, "destination");

        var success = FileService.CopyFilesToDirectoryAsync(sourceFiles, destination).Result;
        Assert.That(success, Is.True);

        var csvFiles = FileService.GetFilesInDirectory(destination, "*.csv");
        var binFiles = FileService.GetFilesInDirectory(destination, "*.bin");
        Assert.That(csvFiles.Count(), Is.EqualTo(2));
        Assert.That(binFiles.Count(), Is.EqualTo(1));
    }

    [TearDown]
    public void Cleanup()
    {
        var csvFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "csv");
        var binFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "bin");

        if (Directory.Exists(csvFilesDirectory))
        {
            Directory.Delete(csvFilesDirectory, true);
        }

        if (Directory.Exists(binFilesDirectory))
        {
            Directory.Delete(binFilesDirectory, true);
        }
    }
}