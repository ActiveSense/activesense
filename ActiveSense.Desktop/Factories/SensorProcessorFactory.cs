using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class SensorProcessorFactory(Func<SensorTypes, ISensorProcessor> processorFactory)
{
    public ISensorProcessor GetSensorProcessor(SensorTypes sensorType)
    {
        return processorFactory.Invoke(sensorType);
    }
}