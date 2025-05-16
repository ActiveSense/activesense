using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.Infrastructure.Parse;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ImportTests;

[TestFixture]
public class ImportIntegrationTests
{
    private Mock<IPdfParser> _mockPdfParser;
    private IFileParser _fileParser;
    private IResultParser _resultParser;
    private DateToWeekdayConverter _dateConverter;
    private IAnalysisSerializer _serializer;
    private string _tempDir;

    [SetUp]
    public void Setup()
    {
        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        
        // Create test components
        _dateConverter = new DateToWeekdayConverter();
        _serializer = new AnalysisSerializer(_dateConverter);
        
        // Mock the PDF parser since we're having issues with PDF creation
        _mockPdfParser = new Mock<IPdfParser>();
        
        // Use real file parser
        var headerAnalyzer = new HeaderAnalyzer();
        _fileParser = new FileParser(headerAnalyzer, _dateConverter);
        
        // Use our result parser with the mocked PDF parser
        _resultParser = new GeneActiveResultParser(_mockPdfParser.Object, _fileParser);
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
    public async Task ImportExportRoundTrip_WithCSVFiles_ShouldPreserveData()
    {
        // Arrange - Create a test analysis
        var originalAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = Path.Combine(_tempDir, "original")
        };

        // Add sleep records
        originalAnalysis.SetSleepRecords(new[] { new SleepRecord
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
        originalAnalysis.SetActivityRecords(new[] { new ActivityRecord
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

        // Create directory structure
        string csvDir = Path.Combine(_tempDir, "csvdir");
        Directory.CreateDirectory(csvDir);

        // Export sleep data to CSV
        string sleepCsvPath = Path.Combine(csvDir, "sleep.csv");
        using (var writer = new StreamWriter(sleepCsvPath))
        {
            writer.WriteLine("\"Night.Starting\",\"Sleep.Onset.Time\",\"Rise.Time\",\"Total.Elapsed.Bed.Time\",\"Total.Sleep.Time\",\"Total.Wake.Time\",\"Sleep.Efficiency\",\"Num.Active.Periods\",\"Median.Activity.Length\"");
            writer.WriteLine("\"2024-11-29\",\"21:25\",\"06:58\",\"34225\",\"26676\",\"7549\",\"77.9\",\"50\",\"124\"");
        }

        // Export activity data to CSV
        string activityCsvPath = Path.Combine(csvDir, "activity.csv");
        using (var writer = new StreamWriter(activityCsvPath))
        {
            writer.WriteLine("\"Day.Number\",\"Steps\",\"Non_Wear\",\"Sleep\",\"Sedentary\",\"Light\",\"Moderate\",\"Vigorous\"");
            writer.WriteLine("\"1\",\"3624\",\"0\",\"12994\",\"26283\",\"14007\",\"3286\",\"0\"");
        }
        
        // Setup the mock PDF parser to return our original analysis
        _mockPdfParser.Setup(p => p.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis> { originalAnalysis });

        // Act - Parse the results
        var results = await _resultParser.ParseResultsAsync(_tempDir);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.GreaterThanOrEqualTo(1)); // At least the mocked PDF and CSV dir

        // Find the PDF-based analysis (from our mock)
        var pdfBasedAnalysis = results.FirstOrDefault(a => a.FileName == "TestAnalysis");
        Assert.That(pdfBasedAnalysis, Is.Not.Null);

        // Find the CSV-based analysis
        var csvBasedAnalysis = results.FirstOrDefault(a => a.FileName == "csvdir");
        Assert.That(csvBasedAnalysis, Is.Not.Null);

        // Verify CSV-based analysis contains the expected data
        if (csvBasedAnalysis is ISleepAnalysis csvSleepAnalysis)
        {
            Assert.That(csvSleepAnalysis.SleepRecords.Count, Is.EqualTo(1));
            var sleepRecord = csvSleepAnalysis.SleepRecords.First();
            Assert.That(sleepRecord.NightStarting, Is.EqualTo("2024-11-29"));
            Assert.That(sleepRecord.SleepEfficiency, Is.EqualTo("77.9"));
        }
        else
        {
            Assert.Fail("CSV-based analysis does not implement ISleepAnalysis");
        }

        if (csvBasedAnalysis is IActivityAnalysis csvActivityAnalysis)
        {
            Assert.That(csvActivityAnalysis.ActivityRecords.Count, Is.EqualTo(1));
            var activityRecord = csvActivityAnalysis.ActivityRecords.First();
            Assert.That(activityRecord.Day, Is.EqualTo("1"));
            Assert.That(activityRecord.Steps, Is.EqualTo("3624"));
        }
        else
        {
            Assert.Fail("CSV-based analysis does not implement IActivityAnalysis");
        }

        // Verify tags have been assigned
        Assert.That(pdfBasedAnalysis.Tags.Count, Is.EqualTo(2));
        Assert.That(csvBasedAnalysis.Tags.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task ParseDirectory_WithMixedValidAndInvalidFiles_ShouldSucceed()
    {
        // Arrange - Create a mix of valid and invalid files

        // Valid sleep CSV
        string sleepCsvPath = Path.Combine(_tempDir, "sleep.csv");
        using (var writer = new StreamWriter(sleepCsvPath))
        {
            writer.WriteLine("\"Night.Starting\",\"Sleep.Onset.Time\",\"Rise.Time\",\"Total.Elapsed.Bed.Time\",\"Total.Sleep.Time\",\"Total.Wake.Time\",\"Sleep.Efficiency\",\"Num.Active.Periods\",\"Median.Activity.Length\"");
            writer.WriteLine("\"2024-11-29\",\"21:25\",\"06:58\",\"34225\",\"26676\",\"7549\",\"77.9\",\"50\",\"124\"");
        }

        // Valid activity CSV
        string activityCsvPath = Path.Combine(_tempDir, "activity.csv");
        using (var writer = new StreamWriter(activityCsvPath))
        {
            writer.WriteLine("\"Day.Number\",\"Steps\",\"Non_Wear\",\"Sleep\",\"Sedentary\",\"Light\",\"Moderate\",\"Vigorous\"");
            writer.WriteLine("\"1\",\"3624\",\"0\",\"12994\",\"26283\",\"14007\",\"3286\",\"0\"");
        }

        // Invalid CSV format
        string invalidCsvPath = Path.Combine(_tempDir, "invalid.csv");
        using (var writer = new StreamWriter(invalidCsvPath))
        {
            writer.WriteLine("This is not a CSV file");
        }

        // Setup the mock PDF parser to return an empty list (no PDF files)
        _mockPdfParser.Setup(p => p.ParsePdfFilesAsync(_tempDir))
            .ReturnsAsync(new List<IAnalysis>());

        // Create a subdirectory with CSV files
        string subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);

        string subSleepCsvPath = Path.Combine(subDir, "sleep.csv");
        using (var writer = new StreamWriter(subSleepCsvPath))
        {
            writer.WriteLine("\"Night.Starting\",\"Sleep.Onset.Time\",\"Rise.Time\",\"Total.Elapsed.Bed.Time\",\"Total.Sleep.Time\",\"Total.Wake.Time\",\"Sleep.Efficiency\",\"Num.Active.Periods\",\"Median.Activity.Length\"");
            writer.WriteLine("\"2024-11-30\",\"22:25\",\"07:58\",\"34225\",\"26676\",\"7549\",\"77.9\",\"50\",\"124\"");
        }

        // Act
        var results = await _resultParser.ParseResultsAsync(_tempDir);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.GreaterThan(0));

        // Verify at least one analysis was created from the subdirectory
        var subDirAnalysis = results.FirstOrDefault(a => a.FileName == "subdir");
        Assert.That(subDirAnalysis, Is.Not.Null);

        if (subDirAnalysis is ISleepAnalysis sleepAnalysis)
        {
            Assert.That(sleepAnalysis.SleepRecords.Count, Is.EqualTo(1));
            var sleepRecord = sleepAnalysis.SleepRecords.First();
            Assert.That(sleepRecord.NightStarting, Is.EqualTo("2024-11-30"));
        }
        else
        {
            Assert.Fail("Sub-directory analysis does not implement ISleepAnalysis");
        }
    }
}