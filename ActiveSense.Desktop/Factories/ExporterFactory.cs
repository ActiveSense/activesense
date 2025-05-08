using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;

namespace ActiveSense.Desktop.Factories;

public class ExporterFactory(Func<SensorTypes, IExporter> exporterFactory)
{
    public IExporter GetExporter(SensorTypes sensorType) => exporterFactory.Invoke(sensorType);
}