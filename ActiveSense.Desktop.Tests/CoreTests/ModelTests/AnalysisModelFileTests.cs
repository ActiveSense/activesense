using System;
using System.Globalization;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using CsvHelper;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.CoreTests.ModelTests;

[TestFixture]
public class AnalysisModeFileTests
{
    [SetUp]
    public void Setup()
    {
        // Initialize the converter
        _dateToWeekdayConverter = new DateToWeekdayConverter();

        // Initialize the analysis model
        _analysis = new GeneActiveAnalysis(_dateToWeekdayConverter);

        // Get directory for test files
        _testsDirectory = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests", "CoreTests",
            "ModelTests", "Files");

        // Load test data from CSV files
        LoadTestData();

        // Set filename for the analysis
        _analysis.FileName = "TestFileAnalysis";
    }

    private GeneActiveAnalysis _analysis;
    private DateToWeekdayConverter _dateToWeekdayConverter;
    private string _testsDirectory;

    private void LoadTestData()
    {
        // Load sleep data
        var sleepFilePath = Path.Combine(_testsDirectory, "sleep.csv");
        if (File.Exists(sleepFilePath))
        {
            using var reader = new StreamReader(sleepFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            _analysis.SetSleepRecords(csv.GetRecords<SleepRecord>().ToList());
        }
        else
        {
            Assert.Fail($"Sleep data file not found at {sleepFilePath}");
        }

        // Load activity data
        var activityFilePath = Path.Combine(_testsDirectory, "activity.csv");
        if (File.Exists(activityFilePath))
        {
            using var reader = new StreamReader(activityFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            _analysis.SetActivityRecords(csv.GetRecords<ActivityRecord>().ToList());
        }
        else
        {
            Assert.Fail($"Activity data file not found at {activityFilePath}");
        }
    }

    [Test]
    public void WhenLoadingFromFiles_ThenSleepRecordsShouldBeCorrectlyParsed()
    {
        // Assert
        Assert.That(_analysis.SleepRecords, Is.Not.Empty, "Sleep records should not be empty");
        Assert.That(_analysis.SleepRecords.Count, Is.EqualTo(9), "Should have 9 sleep records");

        // Check specific values from the first record
        var firstRecord = _analysis.SleepRecords.First();
        Assert.That(firstRecord.NightStarting, Is.EqualTo("2024-11-29"));
        Assert.That(firstRecord.SleepOnsetTime, Is.EqualTo("21:25"));
        Assert.That(firstRecord.SleepEfficiency, Is.EqualTo("77.9"));
    }

    [Test]
    public void WhenLoadingFromFiles_ThenActivityRecordsShouldBeCorrectlyParsed()
    {
        // Assert
        Assert.That(_analysis.ActivityRecords, Is.Not.Empty, "Activity records should not be empty");
        Assert.That(_analysis.ActivityRecords.Count, Is.EqualTo(8), "Should have 8 activity records");

        // Check specific values from the first record
        var firstRecord = _analysis.ActivityRecords.First();
        Assert.That(firstRecord.Day, Is.EqualTo("2024-10-03"));
        Assert.That(firstRecord.Steps, Is.EqualTo("3624"));
        Assert.That(firstRecord.Light, Is.EqualTo("14007"));
    }

    [Test]
    public void TotalSleepTime_ShouldMatchSumFromFile()
    {
        // Calculate expected value manually from file data
        double expected = 26676 + 26998 + 18473 + 22759 + 28335 + 25009 + 26575 + 26575 + 26575;

        // Act
        var actual = _analysis.TotalSleepTime;

        // Assert
        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "TotalSleepTime does not match the sum in the file");
    }

    [Test]
    public void AverageSleepTime_ShouldMatchAverageFromFile()
    {
        // Calculate expected value manually from file data
        double sum = 26676 + 26998 + 18473 + 22759 + 28335 + 25009 + 26575 + 26575 + 26575;
        var expected = sum / 9.0; // 9 records

        // Act
        var actual = _analysis.AverageSleepTime;

        // Assert
        Assert.That(actual, Is.EqualTo(expected).Within(0.01),
            "AverageSleepTime does not match the average in the file");
    }

    [Test]
    public void SleepEfficiency_ShouldMatchValuesFromFile()
    {
        // Expected values from the file
        var expected = new[] { 77.9, 74.2, 73.1, 79.1, 79.7, 80.7, 63.1, 63.1, 63.1 };

        // Act
        var actual = _analysis.SleepEfficiency;

        // Assert
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "SleepEfficiency array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.1),
                $"SleepEfficiency value at index {i} is incorrect");
    }

    [Test]
    public void StepsPerDay_ShouldMatchValuesFromFile()
    {
        // Expected values from the file
        var expected = new[] { 3624.0, 10217.0, 11553.0, 7627.0, 8630.0, 3894.0, 7338.0, 325.0 };

        // Act
        var actual = _analysis.StepsPerDay;

        // Assert
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "StepsPerDay array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"StepsPerDay value at index {i} is incorrect");
    }

    [Test]
    public void GetSleepChartData_ShouldProduceCorrectChartData()
    {
        // Test the implementation via the IChartDataProvider interface
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetSleepDistributionChartData();

        // Assert
        Assert.That(chartData.Title, Is.EqualTo($"{_analysis.FileName}"));
        Assert.That(chartData.Labels.Length, Is.EqualTo(2));
        Assert.That(chartData.Data.Length, Is.EqualTo(2));

        var expectedSleepTime = Math.Round(_analysis.AverageSleepTime / 3600, 1);
        var expectedWakeTime = Math.Round(_analysis.AverageWakeTime / 3600, 1);

        Assert.That(chartData.Data[0], Is.EqualTo(expectedSleepTime).Within(0.01));
        Assert.That(chartData.Data[1], Is.EqualTo(expectedWakeTime).Within(0.01));
    }

    [Test]
    public void GetActivityDistributionChartData_ShouldProduceCorrectChartData()
    {
        // Test the implementation via the IChartDataProvider interface
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetActivityDistributionChartData().ToList();

        // Assert
        Assert.That(chartData.Count, Is.EqualTo(3));

        // Check labels match weekdays
        var expectedLabels = _analysis.ActivityWeekdays();
        for (var i = 0; i < chartData.Count; i++)
        {
            Assert.That(chartData[i].Labels.Length, Is.EqualTo(expectedLabels.Length));
            for (var j = 0; j < expectedLabels.Length; j++)
                Assert.That(chartData[i].Labels[j], Is.EqualTo(expectedLabels[j]));
        }

        // Verify data values
        Assert.That(chartData[0].Title, Is.EqualTo("Leichte Aktivität"));
        for (var i = 0; i < chartData[0].Data.Length; i++)
            Assert.That(chartData[0].Data[i],
                Is.EqualTo(Math.Round(_analysis.LightActivity[i] / 3600, 1)).Within(0.01));

        Assert.That(chartData[1].Title, Is.EqualTo("Mittlere Aktivität"));
        for (var i = 0; i < chartData[1].Data.Length; i++)
            Assert.That(chartData[1].Data[i],
                Is.EqualTo(Math.Round(_analysis.ModerateActivity[i] / 3600, 1)).Within(0.01));

        Assert.That(chartData[2].Title, Is.EqualTo("Intensive Aktivität"));
        for (var i = 0; i < chartData[2].Data.Length; i++)
            Assert.That(chartData[2].Data[i],
                Is.EqualTo(Math.Round(_analysis.VigorousActivity[i] / 3600, 1)).Within(0.01));
    }

    [Test]
    public void GetTotalSleepTimePerDayChartData_ShouldMatchSleepTimeValues()
    {
        // Test the implementation via the IChartDataProvider interface
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetTotalSleepTimePerDayChartData();

        // Assert
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.SleepWeekdays().Length));
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.TotalSleepTimePerDay.Length));

        // Check that the data matches the TotalSleepTimePerDay values
        for (var i = 0; i < _analysis.TotalSleepTimePerDay.Length; i++)
            Assert.That(chartData.Data[i],
                Is.EqualTo(Math.Round(_analysis.TotalSleepTimePerDay[i] / 3600, 1)).Within(0.01));
    }

    [Test]
    public void GetStepsChartData_ShouldMatchStepsPerDayValues()
    {
        // Test the implementation via the IChartDataProvider interface
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetStepsChartData();

        // Assert
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.ActivityWeekdays().Length));

        // Check that the data matches the StepsPerDay values
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.StepsPerDay.Length));
        for (var i = 0; i < _analysis.StepsPerDay.Length; i++)
            Assert.That(chartData.Data[i], Is.EqualTo(_analysis.StepsPerDay[i]).Within(0.01));
    }
}