using System;
using System.Globalization;
using System.IO;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Tests.Helpers;
using CsvHelper;
using FluentAvalonia.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ResultParserTests;

[TestFixture]
public class GeneActiveResultParserTests
{
    private Comparer _comparer;
    private ResultParserFactory _resultParserFactory;
    private ServiceProvider _serviceProvider;
    private string _filesPath;
    private string _outputPath;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<GeneActiveResultParser>();

        services.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new ArgumentException($"No parser found for sensor type {type}")
        });

        services.AddSingleton<ResultParserFactory>();

        _serviceProvider = services.BuildServiceProvider();

        _resultParserFactory = _serviceProvider.GetRequiredService<ResultParserFactory>();

        _comparer = new Comparer();

        _filesPath = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests/ResultParserTests");
        
        _outputPath = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests/TestOutput");
        
        Directory.CreateDirectory(_outputPath);
        
        Console.WriteLine($"Test files path: {_filesPath}");
        Console.WriteLine($"Test output path: {_outputPath}");
    }

    [Test]
    public void ParseActivityResults()
    {
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);

        var results = parser.ParseResultsAsync(_filesPath);
        
        Assert.That(results.Result.Count(), Is.EqualTo(1));

        foreach (var result in results.Result)
        {
            var records = result.ActivityRecords;

            // Create new csv file in the dedicated output directory
            var csvFile = Path.Combine(_outputPath, "activity1.csv");
            Console.WriteLine($"Writing activity results to: {csvFile}");
            
            try
            {
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }
                
                Console.WriteLine("Activity CSV file created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing activity CSV: {ex.Message}");
                throw;
            }

            var comparison = _comparer.CompareFiles(csvFile,
                Path.Combine(_filesPath, "Files/activity.csv"));
            Assert.That(comparison, Is.True);
        }
    }

    [Test]
    public void ParseSleepResults()
    {
        var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
        var results = parser.ParseResultsAsync(_filesPath);

        Assert.That(results.Result.Count(), Is.EqualTo(1));
        
        foreach (var result in results.Result)
        {
            var records = result.SleepRecords;

            // Create new csv file in the dedicated output directory
            var csvFile = Path.Combine(_outputPath, "sleep1.csv");
            Console.WriteLine($"Writing sleep results to: {csvFile}");
            
            try
            {
                using (var writer = new StreamWriter(csvFile))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }
                
                Console.WriteLine("Sleep CSV file created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing sleep CSV: {ex.Message}");
                throw;
            }

            var comparison =
                _comparer.CompareFiles(csvFile, Path.Combine(_filesPath, "Files/sleep.csv"));
            Assert.That(comparison, Is.True);
        }
    }

    // [TearDown]
    // public void Cleanup()
    // {
    //     var sleepFile = Path.Combine(_outputPath, "sleep1.csv");
    //     var activityFile = Path.Combine(_outputPath, "activity1.csv");
    //     
    //     try
    //     {
    //         if (File.Exists(sleepFile))
    //         {
    //             File.Delete(sleepFile);
    //             Console.WriteLine($"Deleted: {sleepFile}");
    //         }
    //     
    //         if (File.Exists(activityFile))
    //         {
    //             File.Delete(activityFile);
    //             Console.WriteLine($"Deleted: {activityFile}");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error during cleanup: {ex.Message}");
    //     }
    // }
}