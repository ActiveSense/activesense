using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Factories;
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

        // Register processor classes
        services.AddTransient<GeneActivProcessor>();
        services.AddTransient<GeneActiveResultParser>();

        _serviceProvider = services.BuildServiceProvider();

        _rScriptService = _serviceProvider.GetRequiredService<IScriptService>();
        
        _sensorProcessorFactory = new SensorProcessorFactory(_serviceProvider);
        _resultParserFactory = new ResultParserFactory(_serviceProvider);
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

    [Test]
    public async Task ExecuteRScript()
    {
        // Arrange
        var processor = _sensorProcessorFactory.CreateProcessor(SensorType.GENEActiv);
        Assert.That(processor, Is.Not.Null, "Failed to create GENEActiv processor");

        // Add this before executing the processor
        var scriptPath = _rScriptService.GetScriptPath();
        Assert.That(File.Exists(scriptPath), Is.True, $"R script not found at {scriptPath}");

        // Act
        var arguments = $"-d {Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output/")}";
        var result = await processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.True, $"Script execution failed: {result.Output}");
    }

    [Test]
    public async Task ParseResults()
    {
        var parser = _resultParserFactory.GetParser(SensorType.GENEActiv);
        var outputDirectory = Path.Combine(AppConfig.OutputsDirectoryPath, "rscript_output");
        var results = await parser.ParseResultsAsync(outputDirectory);

        Assert.That(results.Count(), Is.EqualTo(1), "No results found in the output directory");

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