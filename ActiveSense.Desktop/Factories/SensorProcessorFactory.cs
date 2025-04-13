using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;

namespace ActiveSense.Desktop.Factories;

public class SensorProcessorFactory(Func<SensorTypes, ISensorProcessor> processorFactory)
{
    public ISensorProcessor CreateProcessor(SensorTypes types)
    {
        return processorFactory(types);
    }
}