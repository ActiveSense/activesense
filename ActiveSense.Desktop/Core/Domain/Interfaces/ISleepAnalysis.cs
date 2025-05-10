using System.Collections.Generic;
using ActiveSense.Desktop.Core.Domain.Models;

namespace ActiveSense.Desktop.Core.Domain.Interfaces;
public interface ISleepAnalysis : IAnalysis
{
    IReadOnlyCollection<SleepRecord> SleepRecords { get; }

    double TotalSleepTime { get; }
    double TotalWakeTime { get; }
    double AverageSleepTime { get; }
    double AverageWakeTime { get; }
    double[] SleepEfficiency { get; }
    double[] TotalSleepTimePerDay { get; }

    string[] SleepWeekdays();
    string[] SleepDates();


    void SetSleepRecords(IEnumerable<SleepRecord> records);
}
