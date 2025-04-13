using System;
using System.Collections.Generic;
using ActiveSense.Desktop.Sensors;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop.Factories;

public interface IResultParserFactory
{
    IResultParser GetParser(SensorType sensorType);
}

public class ResultParserFactory : IResultParserFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<SensorType, Type> _parserTypes;

    public ResultParserFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _parserTypes = new Dictionary<SensorType, Type>
        {
            { SensorType.GENEActiv, typeof(GeneActiveResultParser) },
            // Add more types
        };
    }

    public IResultParser GetParser(SensorType sensorType)
    {
        if (_parserTypes.TryGetValue(sensorType, out var parserType))
        {
            return (IResultParser)_serviceProvider.GetRequiredService(parserType);
        }

        throw new ArgumentException($"No parser found for sensor type {sensorType}");
    }
}