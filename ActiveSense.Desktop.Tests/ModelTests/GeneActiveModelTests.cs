using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ModelTests;

[TestFixture]
public class AnalysisModelTests
{
    private GeneActiveAnalysis _analysis;
    private DateToWeekdayConverter _dateToWeekdayConverter;
    
    // Sample data
    private List<SleepRecord> _sampleSleepRecords;
    private List<ActivityRecord> _sampleActivityRecords;
    
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
    
    #region Helper Methods
    
    private List<SleepRecord> LoadSampleSleepRecords()
    {
        return new List<SleepRecord>
        {
            new SleepRecord
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
            new SleepRecord
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
            new SleepRecord
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
            new ActivityRecord
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
            new ActivityRecord
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
            new ActivityRecord
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
    
    #endregion
    
    #region Sleep Metrics Tests
    
    [Test]
    public void TotalSleepTime_ShouldCalculateCorrectSum()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        double expected = 26676 + 26998 + 18473; // Sum of all sleep times
        
        double actual = sleepAnalysis.TotalSleepTime;
        
        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "TotalSleepTime calculation is incorrect");
    }
    
    [Test]
    public void TotalWakeTime_ShouldCalculateCorrectSum()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        double expected = 7549 + 9395 + 6790; // Sum of all wake times
        
        double actual = sleepAnalysis.TotalWakeTime;
        
        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "TotalWakeTime calculation is incorrect");
    }
    
    [Test]
    public void AverageSleepTime_ShouldCalculateCorrectAverage()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        double expected = (26676 + 26998 + 18473) / 3.0; // Average of all sleep times
        
        double actual = sleepAnalysis.AverageSleepTime;
        
        Assert.That(actual, Is.EqualTo(expected).Within(0.01), "AverageSleepTime calculation is incorrect");
    }
    
    [Test]
    public void AverageWakeTime_ShouldCalculateCorrectAverage()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        double expected = (7549 + 9395 + 6790) / 3.0; // Average of all wake times
        
        double actual = sleepAnalysis.AverageWakeTime;
        
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
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"SleepEfficiency value at index {i} is incorrect");
        }
    }
    
    [Test]
    public void TotalSleepTimePerDay_ShouldReturnCorrectValues()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        double[] expected = { 26676, 26998, 18473 };
        
        var actual = sleepAnalysis.TotalSleepTimePerDay;
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "TotalSleepTimePerDay array length is incorrect");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"TotalSleepTimePerDay value at index {i} is incorrect");
        }
    }
    
    #endregion
    
    #region Activity Metrics Tests
    
    [Test]
    public void StepsPerDay_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;
        
        double[] expected = { 3624, 10217, 11553 };
        
        var actual = activityAnalysis.StepsPerDay;
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "StepsPerDay array length is incorrect");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"StepsPerDay value at index {i} is incorrect");
        }
    }
    
    [Test]
    public void ModerateActivity_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;
        
        double[] expected = { 3286, 4440, 9008 };
        
        var actual = activityAnalysis.ModerateActivity;
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "ModerateActivity array length is incorrect");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"ModerateActivity value at index {i} is incorrect");
        }
    }
    
    [Test]
    public void VigorousActivity_ShouldReturnCorrectValues()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;
        
        double[] expected = { 0, 2076, 0 };
        
        var actual = activityAnalysis.VigorousActivity;
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "VigorousActivity array length is incorrect");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"VigorousActivity value at index {i} is incorrect");
        }
    }
    
    [Test]
    public void StepsPercentage_ShouldCalculateCorrectPercentages()
    {
        // Cast to IActivityAnalysis to test the interface method
        IActivityAnalysis activityAnalysis = _analysis;
        
        double[] expected = { 
            3624 / 10000.0 * 100, 
            10217 / 10000.0 * 100, 
            11553 / 10000.0 * 100 
        };
        
        var actual = activityAnalysis.StepsPercentage;
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length), "StepsPercentage array length is incorrect");
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(actual[i], Is.EqualTo(expected[i]).Within(0.01), $"StepsPercentage value at index {i} is incorrect");
        }
    }
    
    #endregion
    
    #region Data Access Method Tests
    
    [Test]
    public void SleepWeekdays_ShouldReturnCorrectWeekdays()
    {
        // Cast to ISleepAnalysis to test the interface method
        ISleepAnalysis sleepAnalysis = _analysis;
        
        var weekdays = sleepAnalysis.SleepWeekdays();
        
        Assert.That(weekdays.Length, Is.EqualTo(3), "SleepWeekdays should return 3 weekdays");
        
        var validWeekdays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        foreach (var day in weekdays)
        {
            string baseDayName = day.Split(' ')[0]; // Remove any counters (e.g., "Monday 2")
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
        
        var validWeekdays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        foreach (var day in weekdays)
        {
            string baseDayName = day.Split(' ')[0]; // Remove any counters (e.g., "Monday 2")
            Assert.That(validWeekdays.Contains(baseDayName, StringComparer.OrdinalIgnoreCase), Is.True, 
                $"{baseDayName} is not a valid weekday");
        }
    }
    
    #endregion
    
    #region Chart Data Tests
    
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
        Assert.That(chartData[0].Data.Length, Is.EqualTo(_analysis.LightActivity.Length), "Light Activity data length is incorrect");
        
        // Second series should be Moderate Activity
        Assert.That(chartData[1].Title, Is.EqualTo("Mittlere Aktivität"), "Second series should be Moderate Activity");
        Assert.That(chartData[1].Data.Length, Is.EqualTo(_analysis.ModerateActivity.Length), "Moderate Activity data length is incorrect");
        
        // Third series should be Vigorous Activity
        Assert.That(chartData[2].Title, Is.EqualTo("Intensive Aktivität"), "Third series should be Vigorous Activity");
        Assert.That(chartData[2].Data.Length, Is.EqualTo(_analysis.VigorousActivity.Length), "Vigorous Activity data length is incorrect");
    }
    
    [Test]
    public void GetSleepChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;
        
        // Act
        var chartData = chartProvider.GetSleepDistributionChartData();
        
        // Assert
        Assert.That(chartData.Title, Is.EqualTo($"Schlafverteilung {_analysis.FileName}"), "Sleep chart title is incorrect");
        Assert.That(chartData.Labels.Length, Is.EqualTo(2), "Sleep chart should have 2 labels");
        Assert.That(chartData.Labels[0], Is.EqualTo("Total Sleep Time"), "First label should be Total Sleep Time");
        Assert.That(chartData.Labels[1], Is.EqualTo("Total Wake Time"), "Second label should be Total Wake Time");
        
        Assert.That(chartData.Data.Length, Is.EqualTo(2), "Sleep chart should have 2 data points");
        Assert.That(chartData.Data[0], Is.EqualTo(_analysis.TotalSleepTime).Within(0.01), "Total Sleep Time data is incorrect");
        Assert.That(chartData.Data[1], Is.EqualTo(_analysis.TotalWakeTime).Within(0.01), "Total Wake Time data is incorrect");
    }
    
    [Test]
    public void GetTotalSleepTimePerDayChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;
        
        // Act
        var chartData = chartProvider.GetTotalSleepTimePerDayChartData();
        
        // Assert
        Assert.That(chartData.Title, Is.EqualTo(_analysis.FileName), "Total sleep time chart title is incorrect");
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.SleepWeekdays().Length), "Total sleep time chart labels length is incorrect");
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.TotalSleepTimePerDay.Length), "Total sleep time chart data length is incorrect");
        
        for (int i = 0; i < _analysis.TotalSleepTimePerDay.Length; i++)
        {
            Assert.That(chartData.Data[i], Is.EqualTo(_analysis.TotalSleepTimePerDay[i]).Within(0.01), $"Total sleep time value at index {i} is incorrect");
        }
    }
    
    [Test]
    public void GetStepsChartData_ShouldReturnCorrectData()
    {
        // Cast to IChartDataProvider to test the interface method
        IChartDataProvider chartProvider = _analysis;
        
        // Act
        var chartData = chartProvider.GetStepsChartData();
        
        // Assert
        Assert.That(chartData.Title, Is.EqualTo(_analysis.FileName), "Steps chart title is incorrect");
        Assert.That(chartData.Labels.Length, Is.EqualTo(_analysis.ActivityWeekdays().Length), "Steps chart labels length is incorrect");
        Assert.That(chartData.Data.Length, Is.EqualTo(_analysis.StepsPerDay.Length), "Steps chart data length is incorrect");
        
        for (int i = 0; i < _analysis.StepsPerDay.Length; i++)
        {
            Assert.That(chartData.Data[i], Is.EqualTo(_analysis.StepsPerDay[i]).Within(0.01), $"Steps value at index {i} is incorrect");
        }
    }
    
    #endregion
    
    #region Cache Management Tests
    
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
        Assert.That(updatedSteps.Length, Is.Not.EqualTo(initialSteps.Length), "Cache was not cleared when adding new records");
        Assert.That(updatedSteps.Length, Is.EqualTo(4), "Updated StepsPerDay should have 4 items");
        Assert.That(updatedSteps[3], Is.EqualTo(5000).Within(0.01), "The new step value was not added correctly");
    }
    #endregion
}