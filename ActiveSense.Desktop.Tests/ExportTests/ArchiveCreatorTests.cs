using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ActiveSense.Desktop.Infrastructure.Export;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ExportTests;

[TestFixture]
public class ArchiveCreatorTests
{
    private ArchiveCreator _archiveCreator;
    private string _tempDir;
    private string _tempPdfPath;

    [SetUp]
    public void Setup()
    {
        _archiveCreator = new ArchiveCreator();

        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        // Create a dummy PDF file
        _tempPdfPath = Path.Combine(_tempDir, "test_report.pdf");
        File.WriteAllText(_tempPdfPath, "This is a mock PDF file");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public async Task CreateArchiveAsync_WithValidInputs_CreatesZipArchiveWithExpectedFiles()
    {
        // Arrange
        string outputPath = Path.Combine(_tempDir, "output.zip");
        string fileName = "TestAnalysis";
        string sleepCsv = "Date,SleepTime,WakeTime\n2023-01-01,22:00,06:00";
        string activityCsv = "Date,Steps,Calories\n2023-01-01,10000,2500";

        // Act
        bool result = await _archiveCreator.CreateArchiveAsync(
            outputPath,
            _tempPdfPath,
            fileName,
            sleepCsv,
            activityCsv);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(outputPath), Is.True);

        // Verify the contents of the ZIP file
        using (var archive = ZipFile.OpenRead(outputPath))
        {
            // Check expected files are in the archive
            var pdfEntry = archive.GetEntry($"{fileName}_report.pdf");
            var sleepEntry = archive.GetEntry($"{fileName}_sleep.csv");
            var activityEntry = archive.GetEntry($"{fileName}_activity.csv");

            Assert.That(pdfEntry, Is.Not.Null, "PDF file entry missing from archive");
            Assert.That(sleepEntry, Is.Not.Null, "Sleep CSV entry missing from archive");
            Assert.That(activityEntry, Is.Not.Null, "Activity CSV entry missing from archive");

            // Verify sleep CSV content
            using (var reader = new StreamReader(sleepEntry.Open()))
            {
                string content = reader.ReadToEnd();
                Assert.That(content, Is.EqualTo(sleepCsv));
            }

            // Verify activity CSV content
            using (var reader = new StreamReader(activityEntry.Open()))
            {
                string content = reader.ReadToEnd();
                Assert.That(content, Is.EqualTo(activityCsv));
            }
        }
    }

    [Test]
    public async Task CreateArchiveAsync_WithInvalidOutputPath_ReturnsFalse()
    {
        // Arrange
        string invalidPath = Path.Combine(_tempDir, "invalid", "nested", "path", "output.zip");

        // Act
        bool result = await _archiveCreator.CreateArchiveAsync(
            invalidPath,
            _tempPdfPath,
            "TestAnalysis",
            "sleep data",
            "activity data");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CreateArchiveAsync_WithInvalidPdfPath_ReturnsFalse()
    {
        // Arrange
        string outputPath = Path.Combine(_tempDir, "output.zip");
        string invalidPdfPath = Path.Combine(_tempDir, "nonexistent.pdf");

        // Act
        bool result = await _archiveCreator.CreateArchiveAsync(
            outputPath,
            invalidPdfPath,
            "TestAnalysis",
            "sleep data",
            "activity data");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CreateArchiveAsync_WithEmptyCsvData_CreatesValidArchive()
    {
        // Arrange
        string outputPath = Path.Combine(_tempDir, "output.zip");
        string fileName = "TestAnalysis";
        string emptySleepCsv = "";
        string emptyActivityCsv = "";

        // Act
        bool result = await _archiveCreator.CreateArchiveAsync(
            outputPath,
            _tempPdfPath,
            fileName,
            emptySleepCsv,
            emptyActivityCsv);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(outputPath), Is.True);

        // Verify the contents of the ZIP file
        using (var archive = ZipFile.OpenRead(outputPath))
        {
            // Check expected files are in the archive with empty content
            var sleepEntry = archive.GetEntry($"{fileName}_sleep.csv");
            var activityEntry = archive.GetEntry($"{fileName}_activity.csv");

            Assert.That(sleepEntry, Is.Not.Null, "Sleep CSV entry missing from archive");
            Assert.That(activityEntry, Is.Not.Null, "Activity CSV entry missing from archive");

            // Verify sleep CSV is empty
            using (var reader = new StreamReader(sleepEntry.Open()))
            {
                string content = reader.ReadToEnd();
                Assert.That(content, Is.Empty);
            }

            // Verify activity CSV is empty
            using (var reader = new StreamReader(activityEntry.Open()))
            {
                string content = reader.ReadToEnd();
                Assert.That(content, Is.Empty);
            }
        }
    }
}