using System.Collections.Generic;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Export.Interfaces;

public interface ICsvExporter
{
    string ExportSleepRecords(IEnumerable<SleepRecord> sleepRecords);
    string ExportActivityRecords(IEnumerable<ActivityRecord> activityRecords);
}