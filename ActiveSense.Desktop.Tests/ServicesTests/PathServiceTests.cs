using System;
using System.IO;
using ActiveSense.Desktop.Core.Services;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ServicesTests;

[TestFixture]
public class PathServiceTests
{
    [SetUp]
    public void Setup()
    {
        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        _pathService = new PathService(_tempDir, _tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch (IOException)
            {
                // Files might be locked, try to delete what we can
                Console.WriteLine("Warning: Could not completely clean up temp directory");
            }
    }

    private PathService _pathService;
    private string _tempDir;

    [Test]
    public void OutputDirectory_ReturnsCorrectValue()
    {
        // Assert
        Assert.That(_pathService.OutputDirectory,
            Is.EqualTo(Path.Combine(_pathService.SolutionBasePath, "AnalysisFiles/")));
    }

    [Test]
    public void ScriptBasePath_WithCustomPath_ReturnsCustomPath()
    {
        // Assert
        Assert.That(_pathService.ScriptBasePath, Is.EqualTo(_tempDir));
    }

    [Test]
    public void EnsureDirectoryExists_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var testPath = Path.Combine(_tempDir, "new_dir");
        Assert.That(Directory.Exists(testPath), Is.False);

        // Act
        var result = _pathService.EnsureDirectoryExists(testPath);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(Directory.Exists(testPath), Is.True);
    }

    [Test]
    public void EnsureDirectoryExists_ReturnsFalseIfDirectoryExists()
    {
        // Arrange
        var testPath = Path.Combine(_tempDir, "existing_dir");
        Directory.CreateDirectory(testPath);
        Assert.That(Directory.Exists(testPath), Is.True);

        // Act
        var result = _pathService.EnsureDirectoryExists(testPath);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(Directory.Exists(testPath), Is.True);
    }

    [Test]
    public void ClearDirectory_RemovesAllFilesAndSubdirectories()
    {
        // Arrange
        var testPath = Path.Combine(_tempDir, "to_clear");
        Directory.CreateDirectory(testPath);

        // Create a file
        File.WriteAllText(Path.Combine(testPath, "test.txt"), "test content");

        // Create a subdirectory
        var subDir = Path.Combine(testPath, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "subtest.txt"), "test content");

        // Act
        _pathService.ClearDirectory(testPath);

        // Assert
        Assert.That(Directory.Exists(testPath), Is.True, "Directory should still exist");
        Assert.That(Directory.GetFiles(testPath), Is.Empty, "All files should be removed");
        Assert.That(Directory.GetDirectories(testPath), Is.Empty, "All subdirectories should be removed");
    }

    [Test]
    public void ClearDirectory_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var testPath = Path.Combine(_tempDir, "nonexistent_dir");
        Assert.That(Directory.Exists(testPath), Is.False);

        // Act
        _pathService.ClearDirectory(testPath);

        // Assert
        Assert.That(Directory.Exists(testPath), Is.True, "Directory should be created");
    }

    [Test]
    public void CombinePaths_CombinesPathsCorrectly()
    {
        // Arrange
        var path1 = "path1";
        var path2 = "path2";
        var path3 = "path3";

        // Act
        var result = _pathService.CombinePaths(path1, path2, path3);

        // Assert
        Assert.That(result, Is.EqualTo(Path.Combine(path1, path2, path3)));
    }
}