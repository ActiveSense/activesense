using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Parse;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ImportTests;

[TestFixture]
public class FileParserTests
{
    private Mock<IHeaderAnalyzer> _mockHeaderAnalyzer;
    private DateToWeekdayConverter _dateConverter;
    private FileParser _fileParser;
    private string _tempDir;

    [SetUp]
    public void Setup()
    {
        _mockHeaderAnalyzer = new Mock<IHeaderAnalyzer>();
        _dateConverter = new DateToWeekdayConverter();
        _fileParser = new FileParser(_mockHeaderAnalyzer.Object, _dateConverter);

        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
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
    public void DetermineAnalysisType_WhenHeadersMatchActivityCsv_ReturnsActivityType()
    {
        // Arrange
        string[] headers = { "Day.Number", "Steps", "Non_Wear", "Sleep", "Sedentary" };
        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(headers)).Returns(true);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(headers)).Returns(false);

        // Act
        var result = _fileParser.DetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Activity));
        _mockHeaderAnalyzer.Verify(x => x.IsActivityCsv(headers), Times.Once);
    }

    [Test]
    public void DetermineAnalysisType_WhenHeadersMatchSleepCsv_ReturnsSleepType()
    {
        // Arrange
        string[] headers = { "Night.Starting", "Sleep.Onset.Time", "Sleep.Efficiency" };
        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(headers)).Returns(false);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(headers)).Returns(true);

        // Act
        var result = _fileParser.DetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Sleep));
        _mockHeaderAnalyzer.Verify(x => x.IsSleepCsv(headers), Times.Once);
    }

    [Test]
    public void DetermineAnalysisType_WhenHeadersDontMatch_ReturnsUnknownType()
    {
        // Arrange
        string[] headers = { "Column1", "Column2", "Column3" };
        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(headers)).Returns(false);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(headers)).Returns(false);

        // Act
        var result = _fileParser.DetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Unknown));
        _mockHeaderAnalyzer.Verify(x => x.IsActivityCsv(headers), Times.Once);
        _mockHeaderAnalyzer.Verify(x => x.IsSleepCsv(headers), Times.Once);
    }

    [Test]
    public void ParseCsvFile_WithActivityCsv_PopulatesActivityRecords()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "activity.csv");
        File.WriteAllText(filePath, @"""Day.Number"",""Steps"",""Non_Wear"",""Sleep"",""Sedentary"",""Light"",""Moderate"",""Vigorous""
""1"",""3624"",""0"",""12994"",""26283"",""14007"",""3286"",""0""");

        var analysis = new GeneActiveAnalysis(_dateConverter);

        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(It.IsAny<string[]>())).Returns(true);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(It.IsAny<string[]>())).Returns(false);

        // Act
        bool result = _fileParser.ParseCsvFile(filePath, analysis);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(analysis.ActivityRecords, Is.Not.Empty);
        Assert.That(analysis.ActivityRecords.Count, Is.EqualTo(1));

        var record = analysis.ActivityRecords.ToArray()[0];
        Assert.That(record.Day, Is.EqualTo("1"));
        Assert.That(record.Steps, Is.EqualTo("3624"));
        Assert.That(record.Light, Is.EqualTo("14007"));
    }

    [Test]
    public void ParseCsvFile_WithSleepCsv_PopulatesSleepRecords()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "sleep.csv");
        File.WriteAllText(filePath, @"""Night.Starting"",""Sleep.Onset.Time"",""Rise.Time"",""Total.Elapsed.Bed.Time"",""Total.Sleep.Time"",""Total.Wake.Time"",""Sleep.Efficiency"",""Num.Active.Periods"",""Median.Activity.Length""
""2024-11-29"",""21:25"",""06:58"",""34225"",""26676"",""7549"",""77.9"",""50"",""124""");

        var analysis = new GeneActiveAnalysis(_dateConverter);

        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(It.IsAny<string[]>())).Returns(false);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(It.IsAny<string[]>())).Returns(true);

        // Act
        bool result = _fileParser.ParseCsvFile(filePath, analysis);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(analysis.SleepRecords, Is.Not.Empty);
        Assert.That(analysis.SleepRecords.Count, Is.EqualTo(1));

        var record = analysis.SleepRecords.ToArray()[0];
        Assert.That(record.NightStarting, Is.EqualTo("2024-11-29"));
        Assert.That(record.SleepOnsetTime, Is.EqualTo("21:25"));
        Assert.That(record.SleepEfficiency, Is.EqualTo("77.9"));
    }

    [Test]
    public void ParseCsvFile_WithUnknownCsv_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "unknown.csv");
        File.WriteAllText(filePath, @"""Column1"",""Column2"",""Column3""
""Value1"",""Value2"",""Value3""");

        var analysis = new GeneActiveAnalysis(_dateConverter);

        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(It.IsAny<string[]>())).Returns(false);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(It.IsAny<string[]>())).Returns(false);

        // Act
        bool result = _fileParser.ParseCsvFile(filePath, analysis);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(analysis.ActivityRecords, Is.Empty);
        Assert.That(analysis.SleepRecords, Is.Empty);
    }

    [Test]
    public void ParseCsvFile_WithInvalidFile_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "invalid.csv");
        File.WriteAllText(filePath, "Not a CSV file");

        var analysis = new GeneActiveAnalysis(_dateConverter);

        // Act
        bool result = _fileParser.ParseCsvFile(filePath, analysis);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ParseCsvDirectoryAsync_WithValidFiles_ReturnsAnalysisObject()
    {
        // Arrange
        string directoryPath = Path.Combine(_tempDir, "valid_directory");
        Directory.CreateDirectory(directoryPath);

        // Create activity file
        string activityFilePath = Path.Combine(directoryPath, "activity.csv");
        File.WriteAllText(activityFilePath, @"""Day.Number"",""Steps"",""Non_Wear"",""Sleep"",""Sedentary"",""Light"",""Moderate"",""Vigorous""
""1"",""3624"",""0"",""12994"",""26283"",""14007"",""3286"",""0""");

        // Create sleep file
        string sleepFilePath = Path.Combine(directoryPath, "sleep.csv");
        File.WriteAllText(sleepFilePath, @"""Night.Starting"",""Sleep.Onset.Time"",""Rise.Time"",""Total.Elapsed.Bed.Time"",""Total.Sleep.Time"",""Total.Wake.Time"",""Sleep.Efficiency"",""Num.Active.Periods"",""Median.Activity.Length""
""2024-11-29"",""21:25"",""06:58"",""34225"",""26676"",""7549"",""77.9"",""50"",""124""");

        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(It.Is<string[]>(h => h.Length > 0 && h[0] == "Day.Number"))).Returns(true);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(It.Is<string[]>(h => h.Length > 0 && h[0] == "Night.Starting"))).Returns(true);

        // Act
        var result = await _fileParser.ParseCsvDirectoryAsync(directoryPath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FileName, Is.EqualTo("valid_directory"));
        Assert.That(result.FilePath, Is.EqualTo(directoryPath));

        if (result is GeneActiveAnalysis analysis)
        {
            Assert.That(analysis.ActivityRecords, Is.Not.Empty);
            Assert.That(analysis.SleepRecords, Is.Not.Empty);
        }
        else
        {
            Assert.Fail("Result should be a GeneActiveAnalysis");
        }
    }

    [Test]
    public async Task ParseCsvDirectoryAsync_WithEmptyDirectory_ReturnsNull()
    {
        // Arrange
        string directoryPath = Path.Combine(_tempDir, "empty_directory");
        Directory.CreateDirectory(directoryPath);

        // Act
        var result = await _fileParser.ParseCsvDirectoryAsync(directoryPath);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ParseCsvDirectoryAsync_WithInvalidFiles_ReturnsNull()
    {
        // Arrange
        string directoryPath = Path.Combine(_tempDir, "invalid_directory");
        Directory.CreateDirectory(directoryPath);

        // Create invalid file
        string invalidFilePath = Path.Combine(directoryPath, "invalid.csv");
        File.WriteAllText(invalidFilePath, "Not a CSV file");

        _mockHeaderAnalyzer.Setup(x => x.IsActivityCsv(It.IsAny<string[]>())).Returns(false);
        _mockHeaderAnalyzer.Setup(x => x.IsSleepCsv(It.IsAny<string[]>())).Returns(false);

        // Act
        var result = await _fileParser.ParseCsvDirectoryAsync(directoryPath);

        // Assert
        Assert.That(result, Is.Null);
    }
}