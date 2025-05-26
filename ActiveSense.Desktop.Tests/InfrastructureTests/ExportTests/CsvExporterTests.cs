using System;
using System.Collections.Generic;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ExportTests;

[TestFixture]
public class CsvExporterTests
{
    [SetUp]
    public void Setup()
    {
        _csvExporter = new CsvExporter();

        // Create sample sleep records
        _sampleSleepRecords = new List<SleepRecord>
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
        };

        // Create sample activity records
        _sampleActivityRecords = new List<ActivityRecord>
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
        };
    }

    private CsvExporter _csvExporter;
    private List<SleepRecord> _sampleSleepRecords;
    private List<ActivityRecord> _sampleActivityRecords;

    [Test]
    public void ExportSleepRecords_WithValidRecords_ReturnsValidCsvString()
    {
        // Act
        var result = _csvExporter.ExportSleepRecords(_sampleSleepRecords);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);

        // Check for CSV header
        Assert.That(result, Does.Contain("Night.Starting"));
        Assert.That(result, Does.Contain("Sleep.Onset.Time"));
        Assert.That(result, Does.Contain("Sleep.Efficiency"));

        // Check for data rows
        Assert.That(result, Does.Contain("2024-11-29"));
        Assert.That(result, Does.Contain("21:25"));
        Assert.That(result, Does.Contain("77.9"));

        Assert.That(result, Does.Contain("2024-11-30"));
        Assert.That(result, Does.Contain("21:55"));
        Assert.That(result, Does.Contain("74.2"));
    }

    [Test]
    public void ExportActivityRecords_WithValidRecords_ReturnsValidCsvString()
    {
        // Act
        var result = _csvExporter.ExportActivityRecords(_sampleActivityRecords);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);

        // Check for CSV header
        Assert.That(result, Does.Contain("Day.Number"));
        Assert.That(result, Does.Contain("Steps"));
        Assert.That(result, Does.Contain("Light"));

        // Check for data rows
        Assert.That(result, Does.Contain("1"));
        Assert.That(result, Does.Contain("3624"));
        Assert.That(result, Does.Contain("14007"));

        Assert.That(result, Does.Contain("2"));
        Assert.That(result, Does.Contain("10217"));
        Assert.That(result, Does.Contain("24346"));
    }

    [Test]
    public void ExportSleepRecords_WithEmptyCollection_ReturnsHeaderOnly()
    {
        // Arrange
        var emptyRecords = new List<SleepRecord>();

        // Act
        var result = _csvExporter.ExportSleepRecords(emptyRecords);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);

        // Should contain header but no data
        Assert.That(result, Does.Contain("Night.Starting"));
        Assert.That(result, Does.Contain("Sleep.Onset.Time"));

        // The headers should be on a single line 
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(1), "Expected only header line for empty collection");
    }

    [Test]
    public void ExportActivityRecords_WithEmptyCollection_ReturnsHeaderOnly()
    {
        // Arrange
        var emptyRecords = new List<ActivityRecord>();

        // Act
        var result = _csvExporter.ExportActivityRecords(emptyRecords);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);

        // Should contain header but no data
        Assert.That(result, Does.Contain("Day.Number"));
        Assert.That(result, Does.Contain("Steps"));

        // The headers should be on a single line
        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(1), "Expected only header line for empty collection");
    }
}