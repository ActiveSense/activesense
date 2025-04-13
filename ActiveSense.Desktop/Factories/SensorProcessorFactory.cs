using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Sensors;

namespace ActiveSense.Desktop.Factories;

public interface ISensorProcessorFactory
{
    ISensorProcessor CreateProcessor(SensorType type);
    IEnumerable<ISensorProcessor> GetAllProcessors();
}

public class SensorProcessorFactory : ISensorProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<SensorType, Type> _processorTypes;

    public SensorProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _processorTypes = new Dictionary<SensorType, Type>
        {
            { SensorType.GENEActiv, typeof(GeneActivProcessor) },
        };
    }

    public ISensorProcessor CreateProcessor(SensorType type)
    {
        if (!_processorTypes.TryGetValue(type, out var processorType))
        {
            throw new ArgumentException($"No processor found for sensor type {type}");
        }

        return (ISensorProcessor)_serviceProvider.GetService(processorType);
    }

    public IEnumerable<ISensorProcessor> GetAllProcessors()
    {
        return _processorTypes.Keys
            .Select(type => CreateProcessor(type));
    }
}