using System.Collections.Generic;
using ActiveSense.Desktop.Core.Domain.Models;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface ICsvExporter
{
    string ExportSleepRecords(IEnumerable<SleepRecord> sleepRecords);
    string ExportActivityRecords(IEnumerable<ActivityRecord> activityRecords);
}