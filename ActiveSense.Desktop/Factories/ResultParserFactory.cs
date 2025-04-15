using System;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class ResultParserFactory(Func<SensorTypes, IResultParser> parserFactory)
{
    public IResultParser GetParser(SensorTypes sensorType) => parserFactory.Invoke(sensorType);
}