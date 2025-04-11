using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Services;
using NUnit.Framework;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Tests.Helpers;
using CsvHelper;
using FluentAvalonia.Core;
using Assert = NUnit.Framework.Assert;

namespace ActiveSense.Desktop.Tests.Tests;

[TestFixture]
public class FileManagementTests
{
    private IFileService _fileService;

    [SetUp]
    public void Setup()
    {
        _fileService = new FileService();
    }


    [Test]
    public void CopyFilesToDirectoryMultipleTypes()
    {
        var sourceFiles = _fileService.GetFilesInDirectory(AppConfig.InputDirectoryPath, "*.*");
        var csvFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "csv");
        var binFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "bin");

        var success = _fileService.CopyFilesToDirectoryAsync(sourceFiles, binFilesDirectory, csvFilesDirectory).Result;
        Assert.That(success, Is.True);

        var csvFiles = _fileService.GetFilesInDirectory(csvFilesDirectory, "*.csv");
        var binFiles = _fileService.GetFilesInDirectory(binFilesDirectory, "*.bin");
        Assert.That(csvFiles.Count(), Is.EqualTo(2));
        Assert.That(binFiles.Count(), Is.EqualTo(1));
    }

    [TearDown]
    public void Cleanup()
    {
        var csvFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "csv");
        var binFilesDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "bin");

        if (Directory.Exists(csvFilesDirectory))
        {
            Directory.Delete(csvFilesDirectory, true);
        }

        if (Directory.Exists(binFilesDirectory))
        {
            Directory.Delete(binFilesDirectory, true);
        }
    }
}

[TestFixture]
public class ResultParserTests
{
    private IResultParserService _resultParserService;
    private Comparer _comparer;

    [SetUp]
    public void Setup()
    {
        _resultParserService = new ResultParserService();
        _comparer = new Comparer();
    }

    [Test]
    public void ParseActivityResults()
    {
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = _resultParserService.ParseResultsAsync(outputDirectory);

        foreach (var result in results.Result)
        {
                var records = result.ActivityRecords;

                foreach (var record in records)
                {
                    Console.WriteLine(record);
                }

                var csvFile = Path.Combine(AppConfig.OutputsDirectoryPath, "activity1.csv");
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                var comparison = _comparer.CompareFiles(csvFile,
                    Path.Combine(AppConfig.OutputsDirectoryPath, "analysis1/activity.csv"));
                Assert.That(comparison, Is.True);
        }
    }

    [Test]
    public void ParseSleepResults()
    {
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = _resultParserService.ParseResultsAsync(outputDirectory);

        foreach (var result in results.Result)
        {
                var records = result.SleepRecords;

                foreach (var record in records)
                {
                    Console.WriteLine(record);
                }

                var csvFile = Path.Combine(AppConfig.OutputsDirectoryPath, "sleep1.csv");
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                var comparison =
                    _comparer.CompareFiles(csvFile, Path.Combine(AppConfig.OutputsDirectoryPath, "analysis1/sleep.csv"));
                Assert.That(comparison, Is.True);
        }
    }

    [Test]
    public void ParseActivityResults_DifferentFiles()
    {
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = _resultParserService.ParseResultsAsync(outputDirectory);

        foreach (var result in results.Result)
        {
                var records = result.ActivityRecords;

                var csvFile = Path.Combine(AppConfig.OutputsDirectoryPath, "activity1.csv");
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                var comparison = _comparer.CompareFiles(csvFile,
                    Path.Combine(AppConfig.DiffsDirectoryPath, "activity_diff.csv"));
                Assert.That(comparison, Is.False);
        }
    }

    [Test]
    public void ParseSleepResults_DifferentFiles()
    {
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = _resultParserService.ParseResultsAsync(outputDirectory);

        foreach (var result in results.Result)
        {
                var records = result.SleepRecords;

                var csvFile = Path.Combine(AppConfig.OutputsDirectoryPath, "sleep1.csv");
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                var comparison =
                    _comparer.CompareFiles(csvFile, Path.Combine(AppConfig.DiffsDirectoryPath, "sleep_diff.csv"));
                Assert.That(comparison, Is.False);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        var sleepFile = Path.Combine(AppConfig.OutputsDirectoryPath, "sleep1.csv");
        var activityFile = Path.Combine(AppConfig.OutputsDirectoryPath, "activity1.csv");
        if (File.Exists(sleepFile))
        {
            File.Delete(sleepFile);
        }

        if (File.Exists(activityFile))
        {
            File.Delete(activityFile);
        }
    }
}

[TestFixture]
public class TestCompleteAnalysisProcess
{
    private IRScriptService _scriptService;
    private IFileService _fileService;
    private IResultParserService _resultParserService;

    [SetUp]
    public void Setup()
    {
        Directory.CreateDirectory(Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output"));
        _scriptService = new RScriptService("Rscript");
        _fileService = new FileService();
        _resultParserService = new ResultParserService();
        Directory.CreateDirectory(Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output"));
    }

    [Test]
    public void CopyFilesToDirectory()
    {
        var sourceFiles = _fileService.GetFilesInDirectory(AppConfig.InputDirectoryPath, "*.bin");

        var success = _fileService
            .CopyFilesToDirectoryAsync(sourceFiles, _scriptService.GetRDataPath(), AppConfig.OutputsDirectoryPath)
            .Result;
        Assert.That(success, Is.True, "Failed to copy files to directory");
    }

    [Test]
    public async Task ExecuteRScript()
    {
        var rScriptPath = Path.Combine(_scriptService.GetRScriptBasePath(), "_main.R");
        var arguments = $"-d {Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output/")}";
        var (scriptSuccess, output, error) = await _scriptService.ExecuteScriptAsync(
            rScriptPath, _scriptService.GetRScriptBasePath(), arguments);
        
        Assert.That(scriptSuccess, Is.True);
    }

    [Test]
    public async Task ParseResults()
    {
        var outputDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output");
        var results = await _resultParserService.ParseResultsAsync(outputDirectory);
        
        Assert.That(results.Count, Is.EqualTo(1), "No results found in the output directory");
    
        foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.FileName}, Type: {result.FileName}");
                foreach (var record in result.ActivityRecords)
                {
                    Console.WriteLine(
                        $"Day: {record.Day}, Steps: {record.Steps}, NonWear: {record.NonWear}, Sleep: {record.Sleep}, Sedentary: {record.Sedentary}, Light: {record.Light}, Moderate: {record.Moderate}, Vigorous: {record.Vigorous}");
                }
                foreach (var record in result.SleepRecords)
                {
                    Console.WriteLine(
                        $"NightStarting: {record.NightStarting}, SleepOnsetTime: {record.SleepOnsetTime}, RiseTime: {record.RiseTime}, TotalElapsedBedTime: {record.TotalElapsedBedTime}, TotalSleepTime: {record.TotalSleepTime}, TotalWakeTime: {record.TotalWakeTime}, SleepEfficiency: {record.SleepEfficiency}, NumActivePeriods: {record.NumActivePeriods}, MedianActivityLength: {record.MedianActivityLength}");
                }
        }
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        var outputDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output");
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, true);
        }
    }
}