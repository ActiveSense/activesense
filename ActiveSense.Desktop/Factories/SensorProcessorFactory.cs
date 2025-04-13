using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;

namespace ActiveSense.Desktop.Factories;

public class SensorProcessorFactory(Func<SensorTypes, ISensorProcessor> processorFactory)
{
    public ISensorProcessor GetSensorProcessor(SensorTypes types)
    {
        return processorFactory(types);
    }
}