using System;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class SensorProcessorFactory(Func<SensorTypes, ISensorProcessor> processorFactory)
{
    public ISensorProcessor GetSensorProcessor(SensorTypes sensorType) => processorFactory.Invoke(sensorType);
}