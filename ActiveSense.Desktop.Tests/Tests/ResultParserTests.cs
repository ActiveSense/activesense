using System;
using System.Globalization;
using System.IO;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Tests.Helpers;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.Tests;

[TestFixture]
public class ResultParserTests
{
    private Comparer _comparer;
    private ResultParserFactory _resultParserFactory;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Register parsers
        services.AddTransient<GeneActiveResultParser>();
        
        // Register factory method for ResultParserFactory
        services.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new ArgumentException($"No parser found for sensor type {type}")
        });
        
        // Register the factory that uses the factory method
        services.AddSingleton<ResultParserFactory>();
        
        _serviceProvider = services.BuildServiceProvider();

        // Get the properly configured factory
        _resultParserFactory = _serviceProvider.GetRequiredService<ResultParserFactory>();
        
        _comparer = new Comparer();
    }

    [Test]
    public void ParseActivityResults()
    {
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = parser.ParseResultsAsync(outputDirectory);

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
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = parser.ParseResultsAsync(outputDirectory);

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
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = parser.ParseResultsAsync(outputDirectory);

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
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var outputDirectory = AppConfig.OutputsDirectoryPath;
        var results = parser.ParseResultsAsync(outputDirectory);

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