using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Import.Interfaces;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class ResultParserFactory(Func<SensorTypes, IResultParser> parserFactory)
{
    public IResultParser GetParser(SensorTypes sensorType) => parserFactory.Invoke(sensorType);
}