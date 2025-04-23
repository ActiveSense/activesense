using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class ExporterFactory(Func<SensorTypes, IResultParser> exporterFactory)
{
    public IResultParser GetExporter(SensorTypes sensorType) => exporterFactory.Invoke(sensorType);
}