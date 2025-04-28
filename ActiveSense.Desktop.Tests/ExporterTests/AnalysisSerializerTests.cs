using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ActiveSense.Desktop.Tests.ModelTests;

[TestFixture]
public class AnalysisSerializerTests
{
    private Analysis _analysis;
    private DateToWeekdayConverter _dateToWeekdayConverter;
    private AnalysisSerializer serializer;
    
    [SetUp]
    public void Setup()
    {
        _dateToWeekdayConverter = new DateToWeekdayConverter();
        _analysis = new Analysis(_dateToWeekdayConverter);
        serializer = new AnalysisSerializer(_dateToWeekdayConverter);
        // Setup test data
        _analysis.FileName = "TestAnalysis";
        _analysis.FilePath = "/test/path/to/analysis";
        
        // Add some sleep records
        _analysis.SetSleepRecords(new List<SleepRecord>
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
            }
        });
        
        // Add some activity records
        _analysis.SetActivityRecords(new List<ActivityRecord>
        {
            new ActivityRecord
            {
                Day = "1",
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
                Day = "2",
                Steps = "10217",
                NonWear = "0",
                Sleep = "26708",
                Sedentary = "29395",
                Light = "24346",
                Moderate = "4440",
                Vigorous = "2076"
            }
        });
    }
    
    [Test]
    public void ExportToBase64_WithValidAnalysis_ReturnsBase64String()
    {
        // Act
        string base64 = serializer.ExportToBase64(_analysis);
        
        // Assert
        Assert.That(base64, Is.Not.Null);
        Assert.That(base64, Is.Not.Empty);
        Assert.DoesNotThrow(() => Convert.FromBase64String(base64), "Result should be a valid Base64 string");
    }
    
    [Test]
    public void ImportFromBase64_WithValidBase64_ReturnsAnalysisObject()
    {
        // Arrange
        string base64 = serializer.ExportToBase64(_analysis);
        
        // Act
        Analysis result = serializer.ImportFromBase64(base64);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FileName, Is.EqualTo(_analysis.FileName));
        Assert.That(result.FilePath, Is.EqualTo(_analysis.FilePath));
    }
    
    [Test]
    public void RoundTrip_PreservesAllRecords()
    {
        // Arrange
        string base64 = serializer.ExportToBase64(_analysis);
        
        // Act
        Analysis result = serializer.ImportFromBase64(base64);
        
        // Assert
        Assert.That(result.ActivityRecords.Count, Is.EqualTo(_analysis.ActivityRecords.Count), "Activity records count should match");
        Assert.That(result.SleepRecords.Count, Is.EqualTo(_analysis.SleepRecords.Count), "Sleep records count should match");
        
        // Verify activity records content
        for (int i = 0; i < _analysis.ActivityRecords.Count; i++)
        {
            var original = _analysis.ActivityRecords.ElementAt(i);
            var deserialized = result.ActivityRecords.ElementAt(i);
            
            Assert.That(deserialized.Day, Is.EqualTo(original.Day));
            Assert.That(deserialized.Steps, Is.EqualTo(original.Steps));
            Assert.That(deserialized.Light, Is.EqualTo(original.Light));
            Assert.That(deserialized.Moderate, Is.EqualTo(original.Moderate));
            Assert.That(deserialized.Vigorous, Is.EqualTo(original.Vigorous));
            Assert.That(deserialized.Sedentary, Is.EqualTo(original.Sedentary));
        }
        
        // Verify sleep records content
        for (int i = 0; i < _analysis.SleepRecords.Count; i++)
        {
            var original = _analysis.SleepRecords.ElementAt(i);
            var deserialized = result.SleepRecords.ElementAt(i);
            
            Assert.That(deserialized.NightStarting, Is.EqualTo(original.NightStarting));
            Assert.That(deserialized.SleepOnsetTime, Is.EqualTo(original.SleepOnsetTime));
            Assert.That(deserialized.TotalSleepTime, Is.EqualTo(original.TotalSleepTime));
            Assert.That(deserialized.SleepEfficiency, Is.EqualTo(original.SleepEfficiency));
        }
    }
    
    [Test]
    public void RoundTrip_PreservesCalculatedMetrics()
    {
        // Arrange
        string base64 = serializer.ExportToBase64(_analysis);
        
        // Act
        Analysis result = serializer.ImportFromBase64(base64);
        
        // Assert - Check that calculated metrics match
        Assert.That(result.TotalSleepTime, Is.EqualTo(_analysis.TotalSleepTime).Within(0.01));
        Assert.That(result.TotalWakeTime, Is.EqualTo(_analysis.TotalWakeTime).Within(0.01));
        Assert.That(result.AverageSleepTime, Is.EqualTo(_analysis.AverageSleepTime).Within(0.01));
        
        // Check arrays
        CollectionAssert.AreEqual(_analysis.StepsPerDay, result.StepsPerDay);
        CollectionAssert.AreEqual(_analysis.SleepEfficiency, result.SleepEfficiency);
    }
    
    // [Test]
    // public void ExportToBase64_WithNullAnalysis_ThrowsArgumentNullException()
    // {
    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() => AnalysisSerializer.ExportToBase64(null));
    // }
    //
    // [Test]
    // public void ImportFromBase64_WithNullOrEmptyString_ThrowsArgumentNullException()
    // {
    //     // Act & Assert
    //     Assert.Throws<ArgumentNullException>(() => AnalysisSerializer.ImportFromBase64(null));
    //     Assert.Throws<ArgumentNullException>(() => AnalysisSerializer.ImportFromBase64(string.Empty));
    // }
    //
    // [Test]
    // public void ImportFromBase64_WithInvalidBase64_ThrowsException()
    // {
    //     // Act & Assert
    //     Assert.Throws<Exception>(() => AnalysisSerializer.ImportFromBase64("This is not a valid Base64 string"));
    // }
}