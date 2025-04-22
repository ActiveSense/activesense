using System;
using System.IO;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ModelTests;

[TestFixture]
public class GeneActiveModelTests
{
    
    private ResultParserFactory _resultParserFactory;
    private ServiceProvider _serviceProvider;
    private string _filesPath;
    private string _outputPath;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<GeneActiveResultParser>();
        services.AddTransient<DateToWeekdayConverter>();

        services.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new ArgumentException($"No parser found for sensor type {type}")
        });

        services.AddSingleton<ResultParserFactory>();

        _serviceProvider = services.BuildServiceProvider();

        _resultParserFactory = _serviceProvider.GetRequiredService<ResultParserFactory>();

        _filesPath = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests/ResultParserTests");
        
        _outputPath = Path.Combine(AppConfig.SolutionBasePath, "ActiveSense.Desktop.Tests/TestOutput");
        
        Directory.CreateDirectory(_outputPath);
        
        Console.WriteLine($"Test files path: {_filesPath}");
        Console.WriteLine($"Test output path: {_outputPath}");
    }}