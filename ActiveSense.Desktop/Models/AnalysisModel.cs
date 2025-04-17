using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace ActiveSense.Desktop.Models;

public class Analysis
{
    public List<ActivityRecord> ActivityRecords = new();
    public List<SleepRecord> SleepRecords = new();
    public string FilePath { get; set; }
    public string FileName { get; set; }

    public double TotalSleepTime => SleepRecords
        .Sum(record => double.TryParse(record.TotalSleepTime, out var time) ? time : 0);

    public double TotalWakeTime => SleepRecords
        .Sum(record => double.TryParse(record.TotalWakeTime, out var time) ? time : 0);

    public double AverageSleepTime => SleepRecords.Any()
        ? SleepRecords.Average(record => double.TryParse(record.TotalSleepTime, out var time) ? time : 0)
        : 0;

    public double AverageWakeTime => SleepRecords.Any()
        ? SleepRecords.Average(record => double.TryParse(record.TotalWakeTime, out var time) ? time : 0)
        : 0;
}

public class SleepRecord
{
    [Name("Night.Starting")] public string NightStarting { get; set; }

    [Name("Sleep.Onset.Time")] public string SleepOnsetTime { get; set; }

    [Name("Rise.Time")] public string RiseTime { get; set; }

    [Name("Total.Elapsed.Bed.Time")] public string TotalElapsedBedTime { get; set; }

    [Name("Total.Sleep.Time")] public string TotalSleepTime { get; set; }

    [Name("Total.Wake.Time")] public string TotalWakeTime { get; set; }

    [Name("Sleep.Efficiency")] public string SleepEfficiency { get; set; }

    [Name("Num.Active.Periods")] public string NumActivePeriods { get; set; }

    [Name("Median.Activity.Length")] public string MedianActivityLength { get; set; }
}

public class ActivityRecord
{
    [Name("Day.Number")] public string Day { get; set; }

    [Name("Steps")] public string Steps { get; set; }

    [Name("Non_Wear")] public string NonWear { get; set; }

    [Name("Sleep")] public string Sleep { get; set; }

    [Name("Sedentary")] public string Sedentary { get; set; }

    [Name("Light")] public string Light { get; set; }

    [Name("Moderate")] public string Moderate { get; set; }

    [Name("Vigorous")] public string Vigorous { get; set; }
}