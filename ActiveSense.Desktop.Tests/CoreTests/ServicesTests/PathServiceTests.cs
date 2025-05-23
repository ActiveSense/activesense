using System;
using System.IO;
using ActiveSense.Desktop.Core.Services;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.ServicesTests;

[TestFixture]
public class PathServiceTests
{
    [SetUp]
    public void Setup()
    {
        // Create a temp directory for test operations
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        // Create a test R scripts directory
        _testRScriptsDir = Path.Combine(_tempDir, "RScripts");
        Directory.CreateDirectory(_testRScriptsDir);

        // Create a mock logger
        _mockLogger = new Mock<ILogger>();

        // Create the path service with the mock logger
        _pathService = new PathService(_mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    private PathService _pathService;
    private Mock<ILogger> _mockLogger;
    private string _tempDir;
    private string _testRScriptsDir;

    [Test]
    public void EnsureDirectoryExists_WithNonExistentDirectory_CreatesDirectoryAndReturnsTrue()
    {
        // Arrange
        var testDir = Path.Combine(_tempDir, "new_dir");
        Assert.That(Directory.Exists(testDir), Is.False, "Directory should not exist initially");

        // Act
        var result = _pathService.EnsureDirectoryExists(testDir);

        // Assert
        Assert.That(result, Is.True, "Method should return true when creating directory");
        Assert.That(Directory.Exists(testDir), Is.True, "Directory should be created");
    }

    [Test]
    public void EnsureDirectoryExists_WithExistingDirectory_ReturnsFalse()
    {
        // Arrange
        var testDir = Path.Combine(_tempDir, "existing_dir");
        Directory.CreateDirectory(testDir);
        Assert.That(Directory.Exists(testDir), Is.True, "Directory should exist initially");

        // Act
        var result = _pathService.EnsureDirectoryExists(testDir);

        // Assert
        Assert.That(result, Is.False, "Method should return false for existing directory");
        Assert.That(Directory.Exists(testDir), Is.True, "Directory should still exist");
    }

    [Test]
    public void ClearDirectory_WithExistingDirectory_RemovesContentsAndKeepsDirectory()
    {
        // Arrange
        var testDir = Path.Combine(_tempDir, "to_clear");
        Directory.CreateDirectory(testDir);

        // Create a file in the directory
        File.WriteAllText(Path.Combine(testDir, "test.txt"), "test content");

        // Create a subdirectory
        var subDir = Path.Combine(testDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "subtest.txt"), "test content");

        // Act
        _pathService.ClearDirectory(testDir);

        // Assert
        Assert.That(Directory.Exists(testDir), Is.True, "Directory should still exist");
        Assert.That(Directory.GetFiles(testDir).Length, Is.EqualTo(0), "Directory should not contain any files");
        Assert.That(Directory.GetDirectories(testDir).Length, Is.EqualTo(0),
            "Directory should not contain any subdirectories");
    }

    [Test]
    public void ClearDirectory_WithNonExistentDirectory_CreatesEmptyDirectory()
    {
        // Arrange
        var testDir = Path.Combine(_tempDir, "nonexistent_dir");
        Assert.That(Directory.Exists(testDir), Is.False, "Directory should not exist initially");

        // Act
        _pathService.ClearDirectory(testDir);

        // Assert
        Assert.That(Directory.Exists(testDir), Is.True, "Directory should be created");
        Assert.That(Directory.GetFileSystemEntries(testDir).Length, Is.EqualTo(0), "Directory should be empty");
    }

    [Test]
    public void CombinePaths_WithMultiplePaths_CombinesCorrectly()
    {
        // Arrange
        var path1 = "path1";
        var path2 = "path2";
        var path3 = "path3";

        // Act
        var result = _pathService.CombinePaths(path1, path2, path3);

        // Assert
        Assert.That(result, Is.EqualTo(Path.Combine(path1, path2, path3)),
            "Combined path should match Path.Combine result");
    }

    [Test]
    public void OutputDirectory_IsAccessible()
    {
        // Act
        var outputDir = _pathService.OutputDirectory;

        // Assert
        Assert.That(outputDir, Is.Not.Null.And.Not.Empty, "Output directory should be non-empty");

        // Note: We can't easily test the exact path as it depends on environment, but we can check that the directory exists or can be created
        Assert.That(Directory.Exists(outputDir) || Directory.CreateDirectory(outputDir) != null,
            "Should be able to access or create the output directory");

        if (Directory.Exists(outputDir) &&
            !outputDir.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            try
            {
                Directory.Delete(outputDir);
            }
            catch
            {
            }
    }

    [Test]
    public void ScriptBasePath_IsAccessible()
    {
        // Act
        var scriptPath = _pathService.ScriptBasePath;

        // Assert
        Assert.That(scriptPath, Is.Not.Null.And.Not.Empty, "Script base path should be non-empty");
    }

    [Test]
    public void ScriptInputPath_IsSubdirectoryOfScriptBasePath()
    {
        // Act
        var basePath = _pathService.ScriptBasePath;
        var inputPath = _pathService.ScriptInputPath;

        // Assert
        Assert.That(inputPath, Does.StartWith(basePath), "Input path should be subdirectory of base path");
        Assert.That(inputPath, Does.EndWith("data"), "Input path should end with 'data'");
    }

    [Test]
    public void MainScriptPath_EndsWithExpectedFilename()
    {
        // Act
        var mainScriptPath = _pathService.MainScriptPath;

        // Assert
        Assert.That(mainScriptPath, Does.EndWith("_main.R"), "Main script path should end with '_main.R'");
    }

    [Test]
    public void ScriptExecutablePath_HandlesCustomRPath()
    {
        // Act - should not throw
        string executablePath = null;
        try
        {
            executablePath = _pathService.ScriptExecutablePath;
        }
        catch (FileNotFoundException)
        {
            // This is expected if R is not installed, we'll skip this test
            Assert.Ignore("R is not installed on this system, skipping test");
        }

        // Assert - if we get here, it should be non-empty
        if (executablePath != null) Assert.That(executablePath, Is.Not.Null.And.Not.Empty);
    }
}