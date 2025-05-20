using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Parse;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ImportTests;

[TestFixture]
public class GeneActiveResultParserTests
{
    private Mock<IPdfParser> _mockPdfParser;
    private Mock<IFileParser> _mockFileParser;
    private GeneActiveResultParser _resultParser;
    private string _tempDir;
    private DateToWeekdayConverter _dateConverter;
    private Mock<ILogger> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockPdfParser = new Mock<IPdfParser>();
        _mockFileParser = new Mock<IFileParser>();
        _mockLogger = new Mock<ILogger>();
        _resultParser = new GeneActiveResultParser(_mockPdfParser.Object, _mockFileParser.Object, _mockLogger.Object);
        _dateConverter = new DateToWeekdayConverter();

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
    public void GetAnalysisPages_ReturnsExpectedPages()
    {
        // Act
        var result = _resultParser.GetAnalysisPages();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result, Does.Contain(ApplicationPageNames.Aktivität));
        Assert.That(result, Does.Contain(ApplicationPageNames.Schlaf));
        Assert.That(result, Does.Contain(ApplicationPageNames.Allgemein));
    }

    [Test]
    public async Task ParseResultsAsync_WithNonExistentDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempDir, "non_existent");

        // Act
        var result = await _resultParser.ParseResultsAsync(nonExistentPath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ParseResultsAsync_WithValidPdfsAndDirectories_ReturnsAnalyses()
    {
        // Arrange
        var pdfAnalysis1 = new GeneActiveAnalysis(_dateConverter) { FileName = "pdf1" };
        var pdfAnalysis2 = new GeneActiveAnalysis(_dateConverter) { FileName = "pdf2" };
        var csvAnalysis1 = new GeneActiveAnalysis(_dateConverter) { FileName = "csv1" };
        var csvAnalysis2 = new GeneActiveAnalysis(_dateConverter) { FileName = "csv2" };

        // Setup PDF parser to return analyses
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { pdfAnalysis1, pdfAnalysis2 });

        // Setup directories for CSV parsing
        string dir1 = Path.Combine(_tempDir, "dir1");
        string dir2 = Path.Combine(_tempDir, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        // Setup file parser to return analyses for the directories
        _mockFileParser.Setup(x => x.ParseCsvDirectoryAsync(dir1))
            .ReturnsAsync(csvAnalysis1);
        _mockFileParser.Setup(x => x.ParseCsvDirectoryAsync(dir2))
            .ReturnsAsync(csvAnalysis2);

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(4));
        Assert.That(result, Does.Contain(pdfAnalysis1));
        Assert.That(result, Does.Contain(pdfAnalysis2));
        Assert.That(result, Does.Contain(csvAnalysis1));
        Assert.That(result, Does.Contain(csvAnalysis2));
    }

    [Test]
    public async Task ParseResultsAsync_WhenPdfParserReturnsEmptyList_OnlyReturnsDirectoryAnalyses()
    {
        // Arrange
        var csvAnalysis = new GeneActiveAnalysis(_dateConverter) { FileName = "csv" };

        // Setup PDF parser to return empty list
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis>());

        // Setup directory for CSV parsing
        string dir = Path.Combine(_tempDir, "dir");
        Directory.CreateDirectory(dir);

        // Setup file parser to return analysis for the directory
        _mockFileParser.Setup(x => x.ParseCsvDirectoryAsync(dir))
            .ReturnsAsync(csvAnalysis);

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result, Does.Contain(csvAnalysis));
    }

    [Test]
    public async Task ParseResultsAsync_WhenDirectoryParsersReturnNull_OnlyReturnsPdfAnalyses()
    {
        // Arrange
        var pdfAnalysis = new GeneActiveAnalysis(_dateConverter) { FileName = "pdf" };

        // Setup PDF parser to return analyses
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { pdfAnalysis });

        // Setup directory for CSV parsing
        string dir = Path.Combine(_tempDir, "dir");
        Directory.CreateDirectory(dir);

        // Setup file parser to return null for the directory
        _mockFileParser.Setup(x => x.ParseCsvDirectoryAsync(dir))
            .ReturnsAsync((IAnalysis)null);

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result, Does.Contain(pdfAnalysis));
    }

    [Test]
    public async Task ParseResultsAsync_ShouldAssignTagsToAnalyses()
    {
        // Arrange
        var analysis = new GeneActiveAnalysis(_dateConverter) { FileName = "test" };

        // Add sleep records
        analysis.SetSleepRecords(new[] { new SleepRecord
        {
            NightStarting = "2024-11-29",
            SleepOnsetTime = "21:25",
            RiseTime = "06:58",
            TotalElapsedBedTime = "34225",
            TotalSleepTime = "26676",
            TotalWakeTime = "7549",
            SleepEfficiency = "77.9",
            NumActivePeriods = "50",
            MedianActivityLength = "124"
        }});

        // Add activity records
        analysis.SetActivityRecords(new[] { new ActivityRecord
        {
            Day = "1",
            Steps = "3624",
            NonWear = "0",
            Sleep = "12994",
            Sedentary = "26283",
            Light = "14007",
            Moderate = "3286",
            Vigorous = "0"
        }});

        // Setup PDF parser to return the analysis
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { analysis });

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);
        var resultAnalysis = result.First();

        // Assert
        Assert.That(resultAnalysis.Tags, Is.Not.Empty);
        Assert.That(resultAnalysis.Tags.Count, Is.EqualTo(2));

        var sleepTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Schlafdaten");
        var activityTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Aktivitätsdaten");

        Assert.That(sleepTag, Is.Not.Null);
        Assert.That(activityTag, Is.Not.Null);

        Assert.That(sleepTag.Color, Is.EqualTo("#3277a8"));
        Assert.That(activityTag.Color, Is.EqualTo("#38a832"));
    }

    [Test]
    public async Task ParseResultsAsync_WithNoSleepData_OnlyAssignsActivityTag()
    {
        // Arrange
        var analysis = new GeneActiveAnalysis(_dateConverter) { FileName = "test" };

        // Add activity records but no sleep records
        analysis.SetActivityRecords(new[] { new ActivityRecord
        {
            Day = "1",
            Steps = "3624",
            NonWear = "0",
            Sleep = "12994",
            Sedentary = "26283",
            Light = "14007",
            Moderate = "3286",
            Vigorous = "0"
        }});

        // Setup PDF parser to return the analysis
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { analysis });

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);
        var resultAnalysis = result.First();

        // Assert
        Assert.That(resultAnalysis.Tags, Is.Not.Empty);
        Assert.That(resultAnalysis.Tags.Count, Is.EqualTo(1));

        var activityTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Aktivitätsdaten");
        Assert.That(activityTag, Is.Not.Null);
        Assert.That(activityTag.Color, Is.EqualTo("#38a832"));

        var sleepTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Schlafdaten");
        Assert.That(sleepTag, Is.Null);
    }

    [Test]
    public async Task ParseResultsAsync_WithNoActivityData_OnlyAssignsSleepTag()
    {
        // Arrange
        var analysis = new GeneActiveAnalysis(_dateConverter) { FileName = "test" };

        // Add sleep records but no activity records
        analysis.SetSleepRecords(new[] { new SleepRecord
        {
            NightStarting = "2024-11-29",
            SleepOnsetTime = "21:25",
            RiseTime = "06:58",
            TotalElapsedBedTime = "34225",
            TotalSleepTime = "26676",
            TotalWakeTime = "7549",
            SleepEfficiency = "77.9",
            NumActivePeriods = "50",
            MedianActivityLength = "124"
        }});

        // Setup PDF parser to return the analysis
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { analysis });

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);
        var resultAnalysis = result.First();

        // Assert
        Assert.That(resultAnalysis.Tags, Is.Not.Empty);
        Assert.That(resultAnalysis.Tags.Count, Is.EqualTo(1));

        var sleepTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Schlafdaten");
        Assert.That(sleepTag, Is.Not.Null);
        Assert.That(sleepTag.Color, Is.EqualTo("#3277a8"));

        var activityTag = resultAnalysis.Tags.FirstOrDefault(t => t.Name == "Aktivitätsdaten");
        Assert.That(activityTag, Is.Null);
    }

    [Test]
    public async Task ParseResultsAsync_WithNoData_AssignsNoTags()
    {
        // Arrange
        var analysis = new GeneActiveAnalysis(_dateConverter) { FileName = "test" };

        // No sleep or activity records

        // Setup PDF parser to return the analysis
        _mockPdfParser.Setup(x => x.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { analysis });

        // Act
        var result = await _resultParser.ParseResultsAsync(_tempDir);
        var resultAnalysis = result.First();

        // Assert
        Assert.That(resultAnalysis.Tags, Is.Empty);
    }
}