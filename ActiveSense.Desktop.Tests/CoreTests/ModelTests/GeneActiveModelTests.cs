using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.CoreTests.ModelTests;

[TestFixture]
public class AnalysisModelTests
{
    [SetUp]
    public void Setup()
    {
        _dateToWeekdayConverter = new DateToWeekdayConverter();

        _analysis = new GeneActiveAnalysis(_dateToWeekdayConverter);

        _sampleSleepRecords = LoadSampleSleepRecords();
        _sampleActivityRecords = LoadSampleActivityRecords();

        _analysis.SetSleepRecords(_sampleSleepRecords);
        _analysis.SetActivityRecords(_sampleActivityRecords);
        _analysis.FileName = "TestAnalysis";
    }

    private GeneActiveAnalysis _analysis;
    private DateToWeekdayConverter _dateToWeekdayConverter;

    // Sample data
    private List<SleepRecord> _sampleSleepRecords;
    private List<ActivityRecord> _sampleActivityRecords;

    private List<SleepRecord> LoadSampleSleepRecords()
    {
        return new List<SleepRecord>
        {
            new()
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
            },
            new()
            {
                NightStarting = "2024-11-30",
                SleepOnsetTime = "21:55",
                RiseTime = "08:03",
                TotalElapsedBedTime = "36393",
                TotalSleepTime = "26998",
                TotalWakeTime = "9395",
                SleepEfficiency = "74.2",
                NumActivePeriods = "67",
                MedianActivityLength = "84"
            },
            new()
            {
                NightStarting = "2024-12-01",
                SleepOnsetTime = "22:12",
                RiseTime = "05:17",
                TotalElapsedBedTime = "25263",
                TotalSleepTime = "18473",
                TotalWakeTime = "6790",
                SleepEfficiency = "73.1",
                NumActivePeriods = "24",
                MedianActivityLength = "165"
            }
        };
    }

    private List<ActivityRecord> LoadSampleActivityRecords()
    {
        return new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-11-29",
                Steps = "3624",
                NonWear = "0",
                Sleep = "12994",
                Sedentary = "26283",
                Light = "14007",
                Moderate = "3286",
                Vigorous = "0"
            },
            new()
            {
                Day = "2024-11-30",
                Steps = "10217",
                NonWear = "0",
                Sleep = "26708",
                Sedentary = "29395",
                Light = "24346",
                Moderate = "4440",
                Vigorous = "2076"
            },
            new()
            {
                Day = "2024-12-01",
                Steps = "11553",
                NonWear = "0",
                Sleep = "28025",
                Sedentary = "31252",
                Light = "17897",
                Moderate = "9008",
                Vigorous = "0"
            }
        };
    }

    [Test]
    public void TotalSleepTime_ShouldCalculateCorrectSum()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        double expected = 26676 + 26998 + 18473; // Sum of all sleep times

        var actual = sleepAnalysis.TotalSleepTime;

        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "TotalSleepTime calculation is incorrect");
    }

    [Test]
    public void TotalWakeTime_ShouldCalculateCorrectSum()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        double expected = 7549 + 9395 + 6790; // Sum of all wake times

        var actual = sleepAnalysis.TotalWakeTime;

        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "TotalWakeTime calculation is incorrect");
    }

    [Test]
    public void AverageSleepTime_ShouldCalculateCorrectAverage()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        var expected = (26676 + 26998 + 18473) / 3.0; // Average of all sleep times

        var actual = sleepAnalysis.AverageSleepTime;

        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "AverageSleepTime calculation is incorrect");
    }

    [Test]
    public void AverageWakeTime_ShouldCalculateCorrectAverage()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        var expected = (7549 + 9395 + 6790) / 3.0; // Average of all wake times

        var actual = sleepAnalysis.AverageWakeTime;

        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "AverageWakeTime calculation is incorrect");
    }

    [Test]
    public void SleepEfficiency_ShouldReturnCorrectValues()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        double[] expected = { 77.9, 74.2, 73.1 };

        var actual = sleepAnalysis.SleepEfficiency;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "SleepEfficiency array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"SleepEfficiency value at index {i} is incorrect");
    }

    [Test]
    public void TotalSleepTimePerDay_ShouldReturnCorrectValues()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        double[] expected = { 26676, 26998, 18473 };

        var actual = sleepAnalysis.TotalSleepTimePerDay;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "TotalSleepTimePerDay array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"TotalSleepTimePerDay value at index {i} is incorrect");
    }

    [Test]
    public void StepsPerDay_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        double[] expected = { 3624, 10217, 11553 };

        var actual = activityAnalysis.StepsPerDay;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "StepsPerDay array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"StepsPerDay value at index {i} is incorrect");
    }

    [Test]
    public void Days_ShouldReturnCorrectValues()
    {
        // Test the "Days" property - this accesses the raw Day values from ActivityRecords
        string[] expected = { "2024-11-29", "2024-11-30", "2024-12-01" };

        var actual = _analysis.Days;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "Days array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]), $"Days value at index {i} is incorrect");
    }

    [Test]
    public void ModerateActivity_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        double[] expected = { 3286, 4440, 9008 };

        var actual = activityAnalysis.ModerateActivity;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "ModerateActivity array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"ModerateActivity value at index {i} is incorrect");
    }

    [Test]
    public void VigorousActivity_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        double[] expected = { 0, 2076, 0 };

        var actual = activityAnalysis.VigorousActivity;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "VigorousActivity array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"VigorousActivity value at index {i} is incorrect");
    }

    [Test]
    public void StepsPercentage_ShouldCalculateCorrectPercentages()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        double[] expected =
        {
            3624 / 10000.0 * 100,
            10217 / 10000.0 * 100,
            11553 / 10000.0 * 100
        };

        var actual = activityAnalysis.StepsPercentage;

        Assert.That(actual.Length, Is.EqualTo(expected.Length), "StepsPercentage array length is incorrect");
        for (var i = 0; i < expected.Length; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01),
                $"StepsPercentage value at index {i} is incorrect");
    }

    [Test]
    public void SleepWeekdays_ShouldReturnCorrectWeekdays()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        var weekdays = sleepAnalysis.SleepWeekdays();

        Assert.That(weekdays.Length, Is.EqualTo(3), "SleepWeekdays should return 3 weekdays");

        var validWeekdays = new[] { "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag" };
        foreach (var day in weekdays)
        {
            var baseDayName = day.Split(' ')[0]; // Remove any counters (e.g., "Monday 2")
            Assert.That(validWeekdays.Contains(baseDayName, StringComparer.OrdinalIgnoreCase), Is.True,
                $"{baseDayName} is not a valid weekday");
        }
    }

    [Test]
    public void ActivityWeekdays_ShouldReturnCorrectWeekdays()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        var weekdays = activityAnalysis.ActivityWeekdays();

        Assert.That(weekdays.Length, Is.EqualTo(3), "ActivityWeekdays should return 3 weekdays");

        var validWeekdays = new[] { "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag" };
        foreach (var day in weekdays)
        {
            var baseDayName = day.Split(' ')[0]; // Remove any counters (e.g., "Monday 2")
            Assert.That(validWeekdays.Contains(baseDayName, StringComparer.OrdinalIgnoreCase), Is.True,
                $"{baseDayName} is not a valid weekday");
        }
    }

    [Test]
    public void ActivityDates_ShouldReturnFormattedDates()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;

        var dates = activityAnalysis.ActivityDates();

        Assert.That(dates.Length, Is.EqualTo(3), "ActivityDates should return 3 dates");

        // Expected dates in dd.MM.yyyy format (default format used in GeneActiveAnalysis)
        string[] expected = { "29.11.2024", "30.11.2024", "01.12.2024" };

        for (var i = 0; i < expected.Length; i++)
            Assert.That(dates[i], Is.EqualTo(expected[i]), $"ActivityDates value at index {i} is incorrect");
    }

    [Test]
    public void SleepDates_ShouldReturnFormattedDates()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;

        var dates = sleepAnalysis.SleepDates();

        Assert.That(dates.Length, Is.EqualTo(3), "SleepDates should return 3 dates");

        // Expected dates in dd.MM.yyyy format (default format used in GeneActiveAnalysis)
        string[] expected = { "29.11.2024", "30.11.2024", "01.12.2024" };

        for (var i = 0; i < expected.Length; i++)
            Assert.That(dates[i], Is.EqualTo(expected[i]), $"SleepDates value at index {i} is incorrect");
    }

    [Test]
    public void ActivityDates_WithInvalidDates_ShouldReturnOriginalStrings()
    {
        // Arrange - Create analysis with invalid date strings
        var invalidActivityRecords = new List<ActivityRecord>
        {
            new()
            {
                Day = "invalid-date-1",
                Steps = "1000",
                NonWear = "0",
                Sleep = "12000",
                Sedentary = "20000",
                Light = "10000",
                Moderate = "3000",
                Vigorous = "500"
            },
            new()
            {
                Day = "not-a-date",
                Steps = "2000",
                NonWear = "0",
                Sleep = "13000",
                Sedentary = "21000",
                Light = "11000",
                Moderate = "3500",
                Vigorous = "600"
            }
        };

        var analysisWithInvalidDates = new GeneActiveAnalysis(_dateToWeekdayConverter);
        analysisWithInvalidDates.SetActivityRecords(invalidActivityRecords);

        // Act
        IActivityAnalysis activityAnalysis = analysisWithInvalidDates;
        var dates = activityAnalysis.ActivityDates();

        // Assert - Should return original strings when parsing fails
        Assert.That(dates.Length, Is.EqualTo(2), "ActivityDates should return 2 dates");
        Assert.That(dates[0], Is.EqualTo("invalid-date-1"), "Should return original string when date parsing fails");
        Assert.That(dates[1], Is.EqualTo("not-a-date"), "Should return original string when date parsing fails");
    }

    [Test]
    public void SleepDates_WithInvalidDates_ShouldReturnOriginalStrings()
    {
        // Arrange - Create analysis with invalid date strings
        var invalidSleepRecords = new List<SleepRecord>
        {
            new()
            {
                NightStarting = "invalid-date-1",
                SleepOnsetTime = "21:00",
                RiseTime = "07:00",
                TotalElapsedBedTime = "36000",
                TotalSleepTime = "28800",
                TotalWakeTime = "7200",
                SleepEfficiency = "80.0",
                NumActivePeriods = "30",
                MedianActivityLength = "120"
            },
            new()
            {
                NightStarting = "not-a-date",
                SleepOnsetTime = "22:00",
                RiseTime = "08:00",
                TotalElapsedBedTime = "36000",
                TotalSleepTime = "28800",
                TotalWakeTime = "7200",
                SleepEfficiency = "80.0",
                NumActivePeriods = "25",
                MedianActivityLength = "140"
            }
        };

        var analysisWithInvalidDates = new GeneActiveAnalysis(_dateToWeekdayConverter);
        analysisWithInvalidDates.SetSleepRecords(invalidSleepRecords);

        // Act
        ISleepAnalysis sleepAnalysis = analysisWithInvalidDates;
        var dates = sleepAnalysis.SleepDates();

        // Assert - Should return original strings when parsing fails
        Assert.That(dates.Length, Is.EqualTo(2), "SleepDates should return 2 dates");
        Assert.That(dates[0], Is.EqualTo("invalid-date-1"), "Should return original string when date parsing fails");
        Assert.That(dates[1], Is.EqualTo("not-a-date"), "Should return original string when date parsing fails");
    }

    [Test]
    public void ActivityDates_WithEmptyRecords_ShouldReturnEmptyArray()
    {
        // Arrange - Create analysis with no activity records
        var emptyAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        emptyAnalysis.SetActivityRecords(new List<ActivityRecord>());

        // Act
        IActivityAnalysis activityAnalysis = emptyAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates, Is.Not.Null, "ActivityDates should not return null");
        Assert.That(dates.Length, Is.EqualTo(0), "ActivityDates should return empty array when no records");
    }

    [Test]
    public void SleepDates_WithEmptyRecords_ShouldReturnEmptyArray()
    {
        // Arrange - Create analysis with no sleep records
        var emptyAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        emptyAnalysis.SetSleepRecords(new List<SleepRecord>());

        // Act
        ISleepAnalysis sleepAnalysis = emptyAnalysis;
        var dates = sleepAnalysis.SleepDates();

        // Assert
        Assert.That(dates, Is.Not.Null, "SleepDates should not return null");
        Assert.That(dates.Length, Is.EqualTo(0), "SleepDates should return empty array when no records");
    }

    [Test]
    public void ActivityDates_WithDifferentDateFormats_ShouldHandleCorrectly()
    {
        // Arrange - Create records with different valid date formats
        var mixedFormatRecords = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-11-29", // ISO format
                Steps = "1000",
                NonWear = "0",
                Sleep = "12000",
                Sedentary = "20000",
                Light = "10000",
                Moderate = "3000",
                Vigorous = "500"
            },
            new()
            {
                Day = "11/30/2024", // US format
                Steps = "2000",
                NonWear = "0",
                Sleep = "13000",
                Sedentary = "21000",
                Light = "11000",
                Moderate = "3500",
                Vigorous = "600"
            },
            new()
            {
                Day = "01.12.2024", // German format (already in target format)
                Steps = "3000",
                NonWear = "0",
                Sleep = "14000",
                Sedentary = "22000",
                Light = "12000",
                Moderate = "4000",
                Vigorous = "700"
            }
        };

        var mixedFormatAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        mixedFormatAnalysis.SetActivityRecords(mixedFormatRecords);

        // Act
        IActivityAnalysis activityAnalysis = mixedFormatAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(3), "ActivityDates should return 3 dates");

        // All should be converted to dd.MM.yyyy format
        Assert.That(dates[0], Is.EqualTo("29.11.2024"), "First date should be formatted correctly");
        Assert.That(dates[1], Is.EqualTo("30.11.2024"), "Second date should be formatted correctly");
        Assert.That(dates[2], Is.EqualTo("12.01.2024"), "Third date should be formatted correctly");
    }

    [Test]
    public void GetActivityDistributionChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetActivityDistributionChartData().ToList();

        // Assert
        Assert.That(chartData.Count, Is.EqualTo(3), "GetActivityDistributionChartData should return 3 series");

        // First series should be Light Activity
        Assert.That(chartData[0].Title, Is.EqualTo("Leichte Aktivität"), "First series should be Light Activity");
        Assert.That(chartData[0].Data.Length, Is.EqualTo(_analysis.LightActivity.Length),
            "Light Activity data length is incorrect");

        // Second series should be Moderate Activity
        Assert.That(chartData[1].Title, Is.EqualTo("Mittlere Aktivität"), "Second series should be Moderate Activity");
        Assert.That(chartData[1].Data.Length, Is.EqualTo(_analysis.ModerateActivity.Length),
            "Moderate Activity data length is incorrect");

        // Third series should be Vigorous Activity
        Assert.That(chartData[2].Title, Is.EqualTo("Intensive Aktivität"), "Third series should be Vigorous Activity");
        Assert.That(chartData[2].Data.Length, Is.EqualTo(_analysis.VigorousActivity.Length),
            "Vigorous Activity data length is incorrect");
    }

    [Test]
    public void GetSleepChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetSleepDistributionChartData();

        // Assert
        Assert.That(chartData.Title, Is.EqualTo($"{_analysis.FileName}"), "Sleep chart title is incorrect");
        Assert.That(chartData.Labels.Length, Is.EqualTo(2), "Sleep chart should have 2 labels");

        Assert.That(chartData.Data.Length, Is.EqualTo(2), "Sleep chart should have 2 data points");
        Assert.That(chartData.Data[0], Is.EqualTo(Math.Round(_analysis.AverageSleepTime / 3600, 1)).Within(0.01),
            "Total Sleep Time data is incorrect");
        Assert.That(chartData.Data[1], Is.EqualTo(Math.Round(_analysis.AverageWakeTime / 3600, 1)).Within(0.01),
            "Total Wake Time data is incorrect");
    }

    [Test]
    public void GetTotalSleepTimePerDayChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetTotalSleepTimePerDayChartData();

        // Assert
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.SleepWeekdays().Length),
            "Total sleep time chart labels length is incorrect");
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.TotalSleepTimePerDay.Length),
            "Total sleep time chart data length is incorrect");

        for (var i = 0; i < _analysis.TotalSleepTimePerDay.Length; i++)
            Assert.That(chartData.Data[i],
                Is.EqualTo(Math.Round(_analysis.TotalSleepTimePerDay[i] / 3600, 1)).Within(0.01),
                $"Total sleep time value at index {i} is incorrect");
    }

    [Test]
    public void GetStepsChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;

        // Act
        var chartData = chartProvider.GetStepsChartData();

        // Assert
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.ActivityWeekdays().Length),
            "Steps chart labels length is incorrect");
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.StepsPerDay.Length),
            "Steps chart data length is incorrect");

        for (var i = 0; i < _analysis.StepsPerDay.Length; i++)
            Assert.That(chartData.Data[i], Is.EqualTo(_analysis.StepsPerDay[i]).Within(0.01),
                $"Steps value at index {i} is incorrect");
    }

    [Test]
    public void ClearCache_ShouldResetCachedValues()
    {
        // Arrange - Call a method to populate the cache
        var initialSteps = _analysis.StepsPerDay;

        // Act - Add more activity records and set them to trigger cache clear
        var newActivityRecord = new ActivityRecord
        {
            Day = "2024-12-02",
            Steps = "5000",
            NonWear = "0",
            Sleep = "20000",
            Sedentary = "25000",
            Light = "15000",
            Moderate = "5000",
            Vigorous = "1000"
        };

        var newRecords = new List<ActivityRecord>(_sampleActivityRecords) { newActivityRecord };
        _analysis.SetActivityRecords(newRecords);

        // Get the new values
        var updatedSteps = _analysis.StepsPerDay;

        // Assert
        Assert.That(updatedSteps.Length, Is.Not.EqualTo(initialSteps.Length),
            "Cache was not cleared when adding new records");
        Assert.That(updatedSteps.Length, Is.EqualTo(4), "Updated StepsPerDay should have 4 items");
        Assert.That(updatedSteps[3], Is.EqualTo(5000).Within(0.01), "The new step value was not added correctly");
    }

    [Test]
    public void Days_WithEmptyActivityRecords_ShouldReturnEmptyArray()
    {
        // Arrange - Create analysis with no activity records
        var emptyAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        emptyAnalysis.SetActivityRecords(new List<ActivityRecord>());

        // Act
        var days = emptyAnalysis.Days;

        // Assert
        Assert.That(days, Is.Not.Null, "Days should not return null");
        Assert.That(days.Length, Is.EqualTo(0), "Days should return empty array when no activity records");
    }

    [Test]
    public void ParseAndFormatDate_WithValidISODate_ShouldFormatCorrectly()
    {
        // Note: ParseAndFormatDate is a private method, so we test it indirectly through ActivityDates()
        // Arrange - Create record with ISO date format
        var recordWithISODate = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-01-15", // ISO format
                Steps = "5000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(recordWithISODate);

        // Act
        IActivityAnalysis activityAnalysis = testAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(1), "Should have one date");
        Assert.That(dates[0], Is.EqualTo("15.01.2024"), "ISO date should be formatted to dd.MM.yyyy");
    }

    [Test]
    public void ParseAndFormatDate_WithLeapYearDate_ShouldHandleCorrectly()
    {
        // Arrange - Test with leap year date (2024 is a leap year)
        var recordWithLeapDate = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-02-29", // Leap year date
                Steps = "6000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(recordWithLeapDate);

        // Act
        IActivityAnalysis activityAnalysis = testAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(1), "Should have one date");
        Assert.That(dates[0], Is.EqualTo("29.02.2024"), "Leap year date should be formatted correctly");
    }

    [Test]
    public void ParseAndFormatDate_WithDifferentFormats_ShouldParseAndFormatConsistently()
    {
        // Arrange - Test various date formats that DateTime.Parse should handle
        var recordsWithVariousFormats = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024/03/15", // Slash format
                Steps = "7000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            },
            new()
            {
                Day = "March 16, 2024", // Long format
                Steps = "7500",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(recordsWithVariousFormats);

        // Act
        IActivityAnalysis activityAnalysis = testAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(2), "Should have two dates");
        Assert.That(dates[0], Is.EqualTo("15.03.2024"), "Slash format should be formatted to dd.MM.yyyy");
        Assert.That(dates[1], Is.EqualTo("16.03.2024"), "Long format should be formatted to dd.MM.yyyy");
    }

    [Test]
    public void SleepDates_WithVariousValidFormats_ShouldFormatConsistently()
    {
        // Arrange - Test sleep records with various date formats
        var recordsWithVariousFormats = new List<SleepRecord>
        {
            new()
            {
                NightStarting = "2024/04/20", // Slash format
                SleepOnsetTime = "22:30",
                RiseTime = "07:30",
                TotalElapsedBedTime = "32400",
                TotalSleepTime = "27000",
                TotalWakeTime = "5400",
                SleepEfficiency = "83.3",
                NumActivePeriods = "20",
                MedianActivityLength = "180"
            },
            new()
            {
                NightStarting = "April 21, 2024", // Long format
                SleepOnsetTime = "23:00",
                RiseTime = "08:00",
                TotalElapsedBedTime = "32400",
                TotalSleepTime = "28800",
                TotalWakeTime = "3600",
                SleepEfficiency = "88.9",
                NumActivePeriods = "15",
                MedianActivityLength = "200"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetSleepRecords(recordsWithVariousFormats);

        // Act
        ISleepAnalysis sleepAnalysis = testAnalysis;
        var dates = sleepAnalysis.SleepDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(2), "Should have two dates");
        Assert.That(dates[0], Is.EqualTo("20.04.2024"), "Slash format should be formatted to dd.MM.yyyy");
        Assert.That(dates[1], Is.EqualTo("21.04.2024"), "Long format should be formatted to dd.MM.yyyy");
    }

    [Test]
    public void ActivityDates_WithCustomFormat_ShouldUseCustomFormat()
    {
        // Note: This tests the private ParseAndFormatDate method indirectly
        // The method uses a constant DateFormat of "dd.MM.yyyy", so we verify this behavior

        // Arrange - Use an analysis with known date
        var record = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-12-25", // Christmas date for easy verification
                Steps = "1000",
                NonWear = "0",
                Sleep = "28800",
                Sedentary = "30000",
                Light = "10000",
                Moderate = "2000",
                Vigorous = "500"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(record);

        // Act
        IActivityAnalysis activityAnalysis = testAnalysis;
        var dates = activityAnalysis.ActivityDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(1), "Should have one date");
        Assert.That(dates[0], Is.EqualTo("25.12.2024"), "Should use dd.MM.yyyy format");
        Assert.That(dates[0], Does.Match(@"^\d{2}\.\d{2}\.\d{4}$"), "Should match dd.MM.yyyy pattern");
    }

    [Test]
    public void SleepDates_ShouldUseConsistentFormat()
    {
        // Arrange - Use sleep analysis with known date
        var record = new List<SleepRecord>
        {
            new()
            {
                NightStarting = "2024-12-31", // New Year's Eve for easy verification
                SleepOnsetTime = "23:30",
                RiseTime = "08:30",
                TotalElapsedBedTime = "32400",
                TotalSleepTime = "28800",
                TotalWakeTime = "3600",
                SleepEfficiency = "88.9",
                NumActivePeriods = "12",
                MedianActivityLength = "250"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetSleepRecords(record);

        // Act
        ISleepAnalysis sleepAnalysis = testAnalysis;
        var dates = sleepAnalysis.SleepDates();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(1), "Should have one date");
        Assert.That(dates[0], Is.EqualTo("31.12.2024"), "Should use dd.MM.yyyy format");
        Assert.That(dates[0], Does.Match(@"^\d{2}\.\d{2}\.\d{4}$"), "Should match dd.MM.yyyy pattern");
    }

    [Test]
    public void Days_ShouldReturnRawDayValues()
    {
        // Arrange - Create records with mixed date formats to verify Days returns raw values
        var mixedRecords = new List<ActivityRecord>
        {
            new()
            {
                Day = "raw-value-1", // Not a date - should be returned as-is
                Steps = "1000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            },
            new()
            {
                Day = "2024-01-01", // Valid date - should still be returned as-is in Days
                Steps = "2000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(mixedRecords);

        // Act
        var days = testAnalysis.Days;

        // Assert
        Assert.That(days.Length, Is.EqualTo(2), "Should have two day values");
        Assert.That(days[0], Is.EqualTo("raw-value-1"), "Should return raw value without formatting");
        Assert.That(days[1], Is.EqualTo("2024-01-01"), "Should return raw value without formatting");
    }

    [Test]
    public void ActivityDates_AndActivityWeekdays_ShouldUseConsistentDateParsing()
    {
        // This test verifies that both ActivityDates() and ActivityWeekdays() 
        // use the same date parsing logic (through ParseAndFormatDate method)

        // Arrange - Use records with specific dates where we know the weekdays
        var recordsWithKnownWeekdays = new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-11-29", // This is a Friday (Freitag in German)
                Steps = "8000",
                NonWear = "0",
                Sleep = "25200",
                Sedentary = "28800",
                Light = "12600",
                Moderate = "3600",
                Vigorous = "900"
            },
            new()
            {
                Day = "2024-11-30", // This is a Saturday (Samstag in German)
                Steps = "5000",
                NonWear = "0",
                Sleep = "32400",
                Sedentary = "25200",
                Light = "10800",
                Moderate = "1800",
                Vigorous = "0"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetActivityRecords(recordsWithKnownWeekdays);

        // Act
        IActivityAnalysis activityAnalysis = testAnalysis;
        var dates = activityAnalysis.ActivityDates();
        var weekdays = activityAnalysis.ActivityWeekdays();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(2), "Should have two dates");
        Assert.That(weekdays.Length, Is.EqualTo(2), "Should have two weekdays");

        // Verify formatted dates
        Assert.That(dates[0], Is.EqualTo("29.11.2024"), "First date should be formatted correctly");
        Assert.That(dates[1], Is.EqualTo("30.11.2024"), "Second date should be formatted correctly");

        // Verify weekdays (should be German weekday names)
        Assert.That(weekdays[0], Does.StartWith("Freitag").Or.StartWith("1. Freitag"),
            "First weekday should be Friday");
        Assert.That(weekdays[1], Does.StartWith("Samstag").Or.StartWith("1. Samstag"),
            "Second weekday should be Saturday");
    }

    [Test]
    public void SleepDates_AndSleepWeekdays_ShouldUseConsistentDateParsing()
    {
        // This test verifies that both SleepDates() and SleepWeekdays() 
        // use the same date parsing logic

        // Arrange - Use records with specific dates where we know the weekdays
        var recordsWithKnownWeekdays = new List<SleepRecord>
        {
            new()
            {
                NightStarting = "2024-12-01", // This is a Sunday (Sonntag in German)
                SleepOnsetTime = "22:00",
                RiseTime = "07:00",
                TotalElapsedBedTime = "32400",
                TotalSleepTime = "28800",
                TotalWakeTime = "3600",
                SleepEfficiency = "88.9",
                NumActivePeriods = "15",
                MedianActivityLength = "200"
            },
            new()
            {
                NightStarting = "2024-12-02", // This is a Monday (Montag in German)
                SleepOnsetTime = "23:00",
                RiseTime = "08:00",
                TotalElapsedBedTime = "32400",
                TotalSleepTime = "27000",
                TotalWakeTime = "5400",
                SleepEfficiency = "83.3",
                NumActivePeriods = "20",
                MedianActivityLength = "180"
            }
        };

        var testAnalysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        testAnalysis.SetSleepRecords(recordsWithKnownWeekdays);

        // Act
        ISleepAnalysis sleepAnalysis = testAnalysis;
        var dates = sleepAnalysis.SleepDates();
        var weekdays = sleepAnalysis.SleepWeekdays();

        // Assert
        Assert.That(dates.Length, Is.EqualTo(2), "Should have two dates");
        Assert.That(weekdays.Length, Is.EqualTo(2), "Should have two weekdays");

        // Verify formatted dates
        Assert.That(dates[0], Is.EqualTo("01.12.2024"), "First date should be formatted correctly");
        Assert.That(dates[1], Is.EqualTo("02.12.2024"), "Second date should be formatted correctly");

        // Verify weekdays (should be German weekday names)
        Assert.That(weekdays[0], Does.StartWith("Sonntag").Or.StartWith("1. Sonntag"),
            "First weekday should be Sunday");
        Assert.That(weekdays[1], Does.StartWith("Montag").Or.StartWith("1. Montag"), "Second weekday should be Monday");
    }
}