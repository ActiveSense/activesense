using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class ResultParserFactory(Func<SensorTypes, IResultParser> parserFactory)
{
    public IResultParser GetParser(SensorTypes sensorType)
    {
        return parserFactory.Invoke(sensorType);
    }
}