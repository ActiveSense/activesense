using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using CsvHelper;

namespace ActiveSense.Desktop.Infrastructure.Export;

public class CsvExporter : ICsvExporter
{
    public string ExportSleepRecords(IEnumerable<SleepRecord> sleepRecords)
    {
        using var stringWriter = new StringWriter();
        using var csv = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

        csv.WriteRecords(sleepRecords);
        csv.Flush();

        return stringWriter.ToString();
    }

    public string ExportActivityRecords(IEnumerable<ActivityRecord> activityRecords)
    {
        using var stringWriter = new StringWriter();
        using var csv = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

        csv.WriteRecords(activityRecords);
        csv.Flush();

        return stringWriter.ToString();
    }
}