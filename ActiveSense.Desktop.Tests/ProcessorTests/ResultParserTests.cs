using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessorTests;

[TestFixture]
public class GeneActiveParserTests
{
    private GeneActiveParser _parser;
    private IResultParser _parserInterface;
    private string _testDataDirectory;
    

    [SetUp]
    public void Setup()
    {
        var dateToWeekdayConverter = new DateToWeekdayConverter();
        _parser = new GeneActiveParser(dateToWeekdayConverter, new AnalysisSerializer(dateToWeekdayConverter));
        _parserInterface = _parser;

        // Create a temporary directory for test data
        _testDataDirectory = Path.Combine(Path.GetTempPath(), "ActiveSenseTest_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the test directory
        if (Directory.Exists(_testDataDirectory))
        {
            try
            {
                Directory.Delete(_testDataDirectory, true);
            }
            catch (Exception)
            {
                // Ignore errors during cleanup
            }
        }
    }

    [Test]
    public void GetAnalysisPages_ShouldReturnExpectedPages()
    {
        // Act
        var pages = _parserInterface.GetAnalysisPages();

        // Assert
        Assert.That(pages, Is.Not.Empty);
        Assert.That(pages, Contains.Item(ApplicationPageNames.Activity));
        Assert.That(pages, Contains.Item(ApplicationPageNames.Sleep));
        Assert.That(pages, Contains.Item(ApplicationPageNames.General));
    }

    [Test]
    public async Task ParseResultsAsync_WithNoDirectories_ReturnsEmptyCollection()
    {
        // Arrange - Use an empty directory

        // Act
        var results = await _parserInterface.ParseResultsAsync(_testDataDirectory);

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task ParseResultsAsync_WithDataDirectory_ParsesDataCorrectly()
    {
        // Arrange
        var sampleDataDir = CreateSampleDataDirectory();

        // Act
        var results = await _parserInterface.ParseResultsAsync(_testDataDirectory);
        var result = results.FirstOrDefault();

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FileName, Is.EqualTo("TestAnalysis"));
        Assert.That(result.FilePath, Is.EqualTo(sampleDataDir));

        // Check sleep records - need to cast to proper interface
        Assert.That(result is ISleepAnalysis, Is.True, "Result should implement ISleepAnalysis");
        var sleepAnalysis = (ISleepAnalysis)result;
        Assert.That(sleepAnalysis.SleepRecords, Is.Not.Empty);
        Assert.That(sleepAnalysis.SleepRecords.Count, Is.EqualTo(2));
        Assert.That(sleepAnalysis.SleepRecords.First().NightStarting, Is.EqualTo("2024-11-29"));
        Assert.That(sleepAnalysis.SleepRecords.First().SleepEfficiency, Is.EqualTo("77.9"));

        // Check activity records - need to cast to proper interface
        Assert.That(result is IActivityAnalysis, Is.True, "Result should implement IActivityAnalysis");
        var activityAnalysis = (IActivityAnalysis)result;
        Assert.That(activityAnalysis.ActivityRecords, Is.Not.Empty);
        Assert.That(activityAnalysis.ActivityRecords.Count, Is.EqualTo(2));
        Assert.That(activityAnalysis.ActivityRecords.First().Day, Is.EqualTo("1"));
        Assert.That(activityAnalysis.ActivityRecords.First().Steps, Is.EqualTo("3624"));
    }

    // [Test]
    // public async Task ParseResultsAsync_WithInvalidCsvFiles_HandlesErrorsGracefully()
    // {
    //     var testDir = Path.Combine(_testDataDirectory, "InvalidData");
    //     Directory.CreateDirectory(testDir);
    //
    //     var invalidCsvPath = Path.Combine(testDir, "invalid.csv");
    //     File.WriteAllText(invalidCsvPath, "This is not a valid CSV file");
    //
    //     try
    //     {
    //         var results = await _parserInterface.ParseResultsAsync(_testDataDirectory);
    //         Assert.That(results, Is.Empty);
    //     }
    //     catch (Exception ex)
    //     {
    //         Assert.That(ex.Message, Does.Contain("Error parsing file")
    //             .Or.Contains("CSV"));
    //     }
    // }

    [Test]
    public void DetermineAnalysisType_WithActivityHeaders_ReturnsActivityType()
    {
        // Arrange
        var headers = new[] { "Day.Number", "Steps", "Light", "Moderate", "Vigorous" };

        // Act
        var result = CallPrivateDetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Activity));
    }

    [Test]
    public void DetermineAnalysisType_WithSleepHeaders_ReturnsSleepType()
    {
        // Arrange
        var headers = new[] { "Night.Starting", "Sleep.Onset.Time", "Total.Sleep.Time", "Sleep.Efficiency" };

        // Act
        var result = CallPrivateDetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Sleep));
    }

    [Test]
    public void DetermineAnalysisType_WithUnknownHeaders_ReturnsUnknownType()
    {
        // Arrange
        var headers = new[] { "Column1", "Column2", "Column3" };

        // Act
        var result = CallPrivateDetermineAnalysisType(headers);

        // Assert
        Assert.That(result, Is.EqualTo(AnalysisType.Unknown));
    }

    #region Helper Methods

    private string CreateSampleDataDirectory()
    {
        var analysisDir = Path.Combine(_testDataDirectory, "TestAnalysis");
        Directory.CreateDirectory(analysisDir);

        // Create sample sleep data
        var sleepCsvPath = Path.Combine(analysisDir, "sleep.csv");
        var sleepData =
            @"""Night.Starting"",""Sleep.Onset.Time"",""Rise.Time"",""Total.Elapsed.Bed.Time"",""Total.Sleep.Time"",""Total.Wake.Time"",""Sleep.Efficiency"",""Num.Active.Periods"",""Median.Activity.Length""
""2024-11-29"",""21:25"",""06:58"",34225,26676,7549,77.9,50,124
""2024-11-30"",""21:55"",""08:03"",36393,26998,9395,74.2,67,84";
        File.WriteAllText(sleepCsvPath, sleepData);

        // Create sample activity data
        var activityCsvPath = Path.Combine(analysisDir, "activity.csv");
        var activityData =
            @"""Day.Number"",""Steps"",""Non_Wear"",""Sleep"",""Sedentary"",""Light"",""Moderate"",""Vigorous""
1,3624,0,12994,26283,14007,3286,0
2,10217,0,26708,29395,24346,4440,2076";
        File.WriteAllText(activityCsvPath, activityData);

        return analysisDir;
    }

    private AnalysisType CallPrivateDetermineAnalysisType(string[] headers)
    {
        // Use reflection to call the private method
        var methodInfo = typeof(GeneActiveParser).GetMethod("DetermineAnalysisType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return (AnalysisType)methodInfo.Invoke(_parser, new object[] { headers });
    }

    #endregion
}