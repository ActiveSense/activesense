using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Process.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class SensorProcessorFactory(Func<SensorTypes, ISensorProcessor> processorFactory)
{
    public ISensorProcessor GetSensorProcessor(SensorTypes sensorType) => processorFactory.Invoke(sensorType);
}