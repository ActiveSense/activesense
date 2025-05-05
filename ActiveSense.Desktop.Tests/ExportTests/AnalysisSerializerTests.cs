using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Export.Implementations;
using ActiveSense.Desktop.Export.Interfaces;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Tests.ExportTests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ActiveSense.Desktop.Tests.ExportTests;

[TestFixture]
public class AnalysisSerializerTests
{
    [SetUp]
    public void Setup()
    {
        _dateToWeekdayConverter = new DateToWeekdayConverter();
        _analysis = new GeneActiveAnalysis(_dateToWeekdayConverter);
        _serializer = new AnalysisSerializer(_dateToWeekdayConverter);

        // Setup test data
        _analysis.FileName = "TestAnalysis";
        _analysis.FilePath = "/test/path/to/analysis";

        // Add some sleep records
        _analysis.SetSleepRecords(new List<SleepRecord>
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
            }
        });

        // Add some activity records
        _analysis.SetActivityRecords(new List<ActivityRecord>
        {
            new()
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
            new()
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

    private GeneActiveAnalysis _analysis;
    private DateToWeekdayConverter _dateToWeekdayConverter;
    private AnalysisSerializer _serializer;

    [Test]
    public void ExportToBase64_WithValidAnalysis_ReturnsBase64String()
    {
        // Act
        var base64 = _serializer.ExportToBase64(_analysis);

        // Assert
        Assert.That(base64, Is.Not.Null);
        Assert.That(base64, Is.Not.Empty);
        Assert.DoesNotThrow(() => Convert.FromBase64String(base64), "Result should be a valid Base64 string");
    }

    [Test]
    public void ImportFromBase64_WithValidBase64_ReturnsAnalysisObject()
    {
        // Arrange
        var base64 = _serializer.ExportToBase64(_analysis);

        // Act
        var result = _serializer.ImportFromBase64(base64);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FileName, Is.EqualTo(_analysis.FileName));
        Assert.That(result.FilePath, Is.EqualTo(_analysis.FilePath));
    }

    [Test]
    public void RoundTrip_PreservesAllRecords()
    {
        // Arrange
        var base64 = _serializer.ExportToBase64(_analysis);

        // Act
        var result = _serializer.ImportFromBase64(base64);

        // Assert - First verify result implements required interfaces
        Assert.That(result is ISleepAnalysis, Is.True, "Result should implement ISleepAnalysis");
        Assert.That(result is IActivityAnalysis, Is.True, "Result should implement IActivityAnalysis");

        var sleepAnalysis = (ISleepAnalysis)result;
        var activityAnalysis = (IActivityAnalysis)result;

        Assert.That(activityAnalysis.ActivityRecords.Count, Is.EqualTo(_analysis.ActivityRecords.Count),
            "Activity records count should match");
        Assert.That(sleepAnalysis.SleepRecords.Count, Is.EqualTo(_analysis.SleepRecords.Count),
            "Sleep records count should match");

        // Verify activity records content
        for (var i = 0; i < _analysis.ActivityRecords.Count; i++)
        {
            var original = _analysis.ActivityRecords.ElementAt(i);
            var deserialized = activityAnalysis.ActivityRecords.ElementAt(i);

            Assert.That(deserialized.Day, Is.EqualTo(original.Day));
            Assert.That(deserialized.Steps, Is.EqualTo(original.Steps));
            Assert.That(deserialized.Light, Is.EqualTo(original.Light));
            Assert.That(deserialized.Moderate, Is.EqualTo(original.Moderate));
            Assert.That(deserialized.Vigorous, Is.EqualTo(original.Vigorous));
            Assert.That(deserialized.Sedentary, Is.EqualTo(original.Sedentary));
        }

        // Verify sleep records content
        for (var i = 0; i < _analysis.SleepRecords.Count; i++)
        {
            var original = _analysis.SleepRecords.ElementAt(i);
            var deserialized = sleepAnalysis.SleepRecords.ElementAt(i);

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
        var base64 = _serializer.ExportToBase64(_analysis);

        // Act
        var result = _serializer.ImportFromBase64(base64);

        // Assert - First verify result implements required interfaces
        Assert.That(result is ISleepAnalysis, Is.True, "Result should implement ISleepAnalysis");
        Assert.That(result is IActivityAnalysis, Is.True, "Result should implement IActivityAnalysis");

        var sleepAnalysis = (ISleepAnalysis)result;
        var activityAnalysis = (IActivityAnalysis)result;

        // Assert - Check that calculated metrics match
        Assert.That(sleepAnalysis.TotalSleepTime, Is.EqualTo(_analysis.TotalSleepTime).Within(0.01));
        Assert.That(sleepAnalysis.TotalWakeTime, Is.EqualTo(_analysis.TotalWakeTime).Within(0.01));
        Assert.That(sleepAnalysis.AverageSleepTime, Is.EqualTo(_analysis.AverageSleepTime).Within(0.01));

        // Check arrays
        CollectionAssert.AreEqual(_analysis.StepsPerDay, activityAnalysis.StepsPerDay);
        CollectionAssert.AreEqual(_analysis.SleepEfficiency, sleepAnalysis.SleepEfficiency);
    }

    [Test]
    public void ExportToBase64_WithInvalidAnalysis_ThrowsArgumentException()
    {
        // Arrange
        var mockInvalidAnalysis = new InvalidAnalysis();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.ExportToBase64(mockInvalidAnalysis));
    }

    [Test]
    public void ImportFromBase64_WithInvalidBase64_ThrowsException()
    {
        // Arrange
        var invalidBase64 = "this-is-not-base64!";

        // Act & Assert
        Assert.Throws<Exception>(() => _serializer.ImportFromBase64(invalidBase64));
    }

    [Test]
    public void ImportFromBase64_WithNullOrEmptyString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.ImportFromBase64(null));
        Assert.Throws<ArgumentNullException>(() => _serializer.ImportFromBase64(string.Empty));
    }
}