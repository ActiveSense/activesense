using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ExporterTests;

[TestFixture]
public class GeneActivExporterTests
{
    [SetUp]
    public void Setup()
    {
        _chartColors = new ChartColors();
        _exporter = new GeneActiveExporter(_chartColors);

        _testOutputPath = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests", "TestOutput");
        Directory.CreateDirectory(_testOutputPath);

        var dateConverter = new DateToWeekdayConverter();
        _sampleAnalysis = CreateSampleAnalysis(dateConverter);
    }

    // [TearDown]
    // public void Cleanup()
    // {
    //     var testFilePath = Path.Combine(_testOutputPath, "test-export.pdf");
    //     if (File.Exists(testFilePath))
    //         try
    //         {
    //             File.Delete(testFilePath);
    //             Console.WriteLine($"Deleted test file: {testFilePath}");
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"Failed to delete test file: {ex.Message}");
    //         }
    // }

    private GeneActiveExporter _exporter;
    private Analysis _sampleAnalysis;
    private string _testOutputPath;
    private ChartColors _chartColors;

    [Test]
    public async Task ExportAsync_WithValidAnalysis_CreatesPdfFile()
    {
        var outputFilePath = Path.Combine(_testOutputPath, "test-export.pdf");

        var result = await _exporter.ExportAsync(_sampleAnalysis, outputFilePath);

        Assert.That(result, Is.True, "Export should return true for successful operation");
        Assert.That(File.Exists(outputFilePath), Is.True, "PDF file should be created");

        var fileInfo = new FileInfo(outputFilePath);
        Assert.That(fileInfo.Length, Is.GreaterThan(0), "PDF file should not be empty");

        Console.WriteLine($"Generated PDF at: {outputFilePath} with size: {fileInfo.Length} bytes");
    }

    [Test]
    public async Task ExportAsync_WithInvalidPath_ReturnsFalse()
    {
        var invalidPath = "Z:\\non\\existent\\drive\\test.pdf";

        var result = await _exporter.ExportAsync(_sampleAnalysis, invalidPath);

        Assert.That(result, Is.False, "Export should return false for invalid path");
    }

    [Test]
    public async Task ExportAsync_ChecksCorrectSensorType()
    {
        Assert.That(_exporter.SupportedType, Is.EqualTo(SensorTypes.GENEActiv));
    }

    private Analysis CreateSampleAnalysis(DateToWeekdayConverter dateConverter)
    {
        var analysis = new Analysis(dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = "/path/to/test"
        };

        analysis.SetSleepRecords(new List<SleepRecord>
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

        analysis.SetActivityRecords(new List<ActivityRecord>
        {
            new()
            {
                Day = "2024-11-29",
                Steps = "8624",
                NonWear = "0",
                Sleep = "25994",
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
            }
        });

        return analysis;
    }
}