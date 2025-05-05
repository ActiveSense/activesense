using System;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Process.Implementations;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessTests;

[TestFixture]
public class FileManagerTests
{
    private FileManager _fileManager;
    private string _tempDir;
    private string _processingDir;
    private string _outputDir;

    [SetUp]
    public void Setup()
    {
        _fileManager = new FileManager();

        // Create temp directory structure for tests
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        _processingDir = Path.Combine(_tempDir, "processing");
        _outputDir = Path.Combine(_tempDir, "output");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
        {
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
    }

    [Test]
    public void ClearDirectory_WithExistingDirectory_RemovesAllContents()
    {
        // Arrange
        Directory.CreateDirectory(_processingDir);
        File.WriteAllText(Path.Combine(_processingDir, "test.txt"), "test content");
        Directory.CreateDirectory(Path.Combine(_processingDir, "subdirectory"));
        File.WriteAllText(Path.Combine(_processingDir, "subdirectory", "subfile.txt"), "subfile content");

        // Verify directory exists and has content
        Assert.That(Directory.Exists(_processingDir), Is.True);
        Assert.That(Directory.GetFiles(_processingDir).Length, Is.EqualTo(1));
        Assert.That(Directory.GetDirectories(_processingDir).Length, Is.EqualTo(1));

        // Act
        _fileManager.ClearDirectory(_processingDir);

        // Assert
        Assert.That(Directory.Exists(_processingDir), Is.False);
    }

    [Test]
    public void ClearDirectory_WithNonExistingDirectory_DoesNotThrow()
    {
        // Arrange
        string nonExistingDir = Path.Combine(_tempDir, "non_existing");
        Assert.That(Directory.Exists(nonExistingDir), Is.False);

        // Act & Assert
        Assert.DoesNotThrow(() => _fileManager.ClearDirectory(nonExistingDir));
    }

    [Test]
    public void CopyFiles_WithSupportedFiles_CopiesOnlySupportedFiles()
    {
        // Arrange
        // Create source files
        string sourceDir = Path.Combine(_tempDir, "source");
        Directory.CreateDirectory(sourceDir);

        string supportedFile1 = Path.Combine(sourceDir, "supported1.bin");
        string supportedFile2 = Path.Combine(sourceDir, "supported2.bin");
        string unsupportedFile = Path.Combine(sourceDir, "unsupported.txt");

        File.WriteAllText(supportedFile1, "supported content 1");
        File.WriteAllText(supportedFile2, "supported content 2");
        File.WriteAllText(unsupportedFile, "unsupported content");

        string[] supportedFileTypes = { ".bin" };

        // Act
        _fileManager.CopyFiles(
            new[] { supportedFile1, supportedFile2, unsupportedFile },
            _processingDir,
            _outputDir,
            supportedFileTypes);

        // Assert
        Assert.That(Directory.Exists(_processingDir), Is.True);
        Assert.That(Directory.Exists(_outputDir), Is.True);

        // Check processing directory contents
        var processedFiles = Directory.GetFiles(_processingDir);
        Assert.That(processedFiles.Length, Is.EqualTo(2));
        Assert.That(processedFiles.Select(Path.GetFileName),
            Does.Contain("supported1.bin").And.Contain("supported2.bin"));
        Assert.That(processedFiles.Select(Path.GetFileName),
            Does.Not.Contain("unsupported.txt"));

        // Check that the contents were copied correctly
        Assert.That(File.ReadAllText(Path.Combine(_processingDir, "supported1.bin")),
            Is.EqualTo("supported content 1"));
        Assert.That(File.ReadAllText(Path.Combine(_processingDir, "supported2.bin")),
            Is.EqualTo("supported content 2"));
    }

    [Test]
    public void CopyFiles_WithPdfFiles_CopiesThemToOutputDirectory()
    {
        // Arrange
        // Create source files
        string sourceDir = Path.Combine(_tempDir, "source");
        Directory.CreateDirectory(sourceDir);

        string pdfFile = Path.Combine(sourceDir, "document.pdf");
        string supportedFile = Path.Combine(sourceDir, "data.bin");

        File.WriteAllText(pdfFile, "%PDF-1.4\nPDF content");
        File.WriteAllText(supportedFile, "supported content");

        string[] supportedFileTypes = { ".bin" };

        // Act
        _fileManager.CopyFiles(
            new[] { pdfFile, supportedFile },
            _processingDir,
            _outputDir,
            supportedFileTypes);

        // Assert
        Assert.That(Directory.Exists(_processingDir), Is.True);
        Assert.That(Directory.Exists(_outputDir), Is.True);

        // Check processing directory contents - should only have bin files
        var processedFiles = Directory.GetFiles(_processingDir);
        Assert.That(processedFiles.Length, Is.EqualTo(1));
        Assert.That(processedFiles.Select(Path.GetFileName), Does.Contain("data.bin"));

        // Check output directory contents - should have PDF files
        var outputFiles = Directory.GetFiles(_outputDir);
        Assert.That(outputFiles.Length, Is.EqualTo(1));
        Assert.That(outputFiles.Select(Path.GetFileName), Does.Contain("document.pdf"));

        // Check that the contents were copied correctly
        Assert.That(File.ReadAllText(Path.Combine(_processingDir, "data.bin")),
            Is.EqualTo("supported content"));
        Assert.That(File.ReadAllText(Path.Combine(_outputDir, "document.pdf")),
            Is.EqualTo("%PDF-1.4\nPDF content"));
    }

    [Test]
    public void CopyFiles_WithNoFiles_CreatesEmptyDirectories()
    {
        // Arrange
        string[] noFiles = Array.Empty<string>();
        string[] supportedFileTypes = { ".bin" };

        // Act
        _fileManager.CopyFiles(noFiles, _processingDir, _outputDir, supportedFileTypes);

        // Assert
        Assert.That(Directory.Exists(_processingDir), Is.True);
        Assert.That(Directory.Exists(_outputDir), Is.True);
        Assert.That(Directory.GetFiles(_processingDir).Length, Is.EqualTo(0));
        Assert.That(Directory.GetFiles(_outputDir).Length, Is.EqualTo(0));
    }

    [Test]
    public void CopyFiles_WithInvalidSourcePath_SkipsInvalidFiles()
    {
        // Arrange
        string validFile = Path.Combine(_tempDir, "valid.bin");
        string invalidFile = Path.Combine(_tempDir, "invalid_dir", "invalid.bin"); // Path doesn't exist

        File.WriteAllText(validFile, "valid content");
        string[] supportedFileTypes = { ".bin" };

        // Act - Should not throw even though one path is invalid
        Assert.DoesNotThrow(() => _fileManager.CopyFiles(
            new[] { validFile, invalidFile },
            _processingDir,
            _outputDir,
            supportedFileTypes));

        // Assert - Valid file should be copied
        Assert.That(Directory.Exists(_processingDir), Is.True);
        var processedFiles = Directory.GetFiles(_processingDir);
        Assert.That(processedFiles.Length, Is.EqualTo(1));
        Assert.That(processedFiles.Select(Path.GetFileName), Does.Contain("valid.bin"));
    }

    [Test]
    public void CopyFiles_WithExistingDestinationFile_OverwritesFile()
    {
        // Arrange
        // Create source file
        string sourceFile = Path.Combine(_tempDir, "data.bin");
        File.WriteAllText(sourceFile, "new content");

        // Create destination directory and existing file
        Directory.CreateDirectory(_processingDir);
        File.WriteAllText(Path.Combine(_processingDir, "data.bin"), "old content");

        string[] supportedFileTypes = { ".bin" };

        // Act
        _fileManager.CopyFiles(
            new[] { sourceFile },
            _processingDir,
            _outputDir,
            supportedFileTypes);

        // Assert
        var processedFiles = Directory.GetFiles(_processingDir);
        Assert.That(processedFiles.Length, Is.EqualTo(1));
        Assert.That(File.ReadAllText(Path.Combine(_processingDir, "data.bin")),
            Is.EqualTo("new content"));
    }

    [Test]
    public void CopyFiles_WithEmptySupportedFileTypes_CopiesNoFiles()
    {
        // Arrange
        string sourceFile = Path.Combine(_tempDir, "data.bin");
        File.WriteAllText(sourceFile, "content");

        string[] emptySupportedFileTypes = Array.Empty<string>();

        // Act
        _fileManager.CopyFiles(
            new[] { sourceFile },
            _processingDir,
            _outputDir,
            emptySupportedFileTypes);

        // Assert
        Assert.That(Directory.Exists(_processingDir), Is.True);
        Assert.That(Directory.GetFiles(_processingDir).Length, Is.EqualTo(0));
    }
}