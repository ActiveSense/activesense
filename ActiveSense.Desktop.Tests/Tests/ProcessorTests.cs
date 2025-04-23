using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.Tests;

[TestFixture]
public class ProcessorTests
{
    private IScriptService _rScriptService;
    private ResultParserFactory _resultParserFactory;
    private SensorProcessorFactory _sensorProcessorFactory;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<IScriptService, RScriptService>();

        // Register processor and parser classes
        services.AddTransient<GeneActivProcessor>();
        services.AddTransient<GeneActiveResultParser>();

        // Register factory methods
        services.AddSingleton<Func<SensorTypes, ISensorProcessor>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActivProcessor>(),
            _ => throw new ArgumentException($"No processor found for sensor type {type}")
        });

        services.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new ArgumentException($"No parser found for sensor type {type}")
        });

        // Register factories that use the factory methods
        services.AddSingleton<SensorProcessorFactory>();
        services.AddSingleton<ResultParserFactory>();

        _serviceProvider = services.BuildServiceProvider();

        // Get the properly configured services and factories
        _rScriptService = _serviceProvider.GetRequiredService<IScriptService>();
        _sensorProcessorFactory = _serviceProvider.GetRequiredService<SensorProcessorFactory>();
        _resultParserFactory = _serviceProvider.GetRequiredService<ResultParserFactory>();
    }

    [Test]
    public async Task CopyFilesToDirectory()
    {
        Assert.That(Directory.Exists(AppConfig.InputDirectoryPath), Is.True, 
            $"Input directory does not exist: {AppConfig.InputDirectoryPath}");
        
        var sourceFiles = FileService.GetFilesInDirectory(AppConfig.InputDirectoryPath, "*.bin");
        
        Assert.That(sourceFiles.Any(), Is.True, "No .bin files found in input directory");
        
        var success = await FileService.CopyFilesToDirectoryAsync(sourceFiles, _rScriptService.GetScriptInputPath());
        Assert.That(success, Is.True, "Failed to copy files to directory");
    }

    // [Test]
    // public async Task ExecuteRScript()
    // {
    //     // Arrange
    //     var processor = _sensorProcessorFactory.GetSensorProcessor(SensorTypes.GENEActiv);
    //     Assert.That(processor, Is.Not.Null, "Failed to create GENEActiv processor");
    //
    //     // Add this before executing the processor
    //     var scriptPath = _rScriptService.GetScriptPath();
    //     Assert.That(File.Exists(scriptPath), Is.True, $"R script not found at {scriptPath}");
    //
    //     // Act
    //     var arguments = $"-d {Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output/")}";
    //     var result = await processor.ProcessAsync(arguments);
    //
    //     // Assert
    //     Assert.That(result.Success, Is.True, $"Script execution failed: {result.Error}");
    // }
    //
    // [Test]
    // public async Task ParseResults()
    // {
    //     var parser = _resultParserFactory.GetParser(SensorTypes.GENEActiv);
    //     var outputDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output");
    //     var results = await parser.ParseResultsAsync(outputDirectory);
    //
    //     var enumerable = results as Analysis[] ?? results.ToArray();
    //     Assert.That(enumerable.Length, Is.GreaterThan(0), "No results found in the output directory");
    //
    //     foreach (var result in enumerable)
    //     {
    //         Console.WriteLine($"Name: {result.FileName}, Path: {result.FilePath}");
    //         
    //         foreach (var record in result.ActivityRecords)
    //         {
    //             Console.WriteLine(
    //                 $"Day: {record.Day}, Steps: {record.Steps}, NonWear: {record.NonWear}, Sleep: {record.Sleep}, " +
    //                 $"Sedentary: {record.Sedentary}, Light: {record.Light}, Moderate: {record.Moderate}, Vigorous: {record.Vigorous}");
    //         }
    //
    //         foreach (var record in result.SleepRecords)
    //         {
    //             Console.WriteLine(
    //                 $"NightStarting: {record.NightStarting}, SleepOnsetTime: {record.SleepOnsetTime}, RiseTime: {record.RiseTime}, " +
    //                 $"TotalElapsedBedTime: {record.TotalElapsedBedTime}, TotalSleepTime: {record.TotalSleepTime}, " +
    //                 $"TotalWakeTime: {record.TotalWakeTime}, SleepEfficiency: {record.SleepEfficiency}, " +
    //                 $"NumActivePeriods: {record.NumActivePeriods}, MedianActivityLength: {record.MedianActivityLength}");
    //         }
    //     }
    // }

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