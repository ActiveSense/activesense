using CsvHelper.Configuration.Attributes;

namespace ActiveSense.Desktop.Core.Domain.Models;

public class SleepRecord
{
    [Name("Night.Starting")] public required string NightStarting { get; set; }

    [Name("Sleep.Onset.Time")] public required string SleepOnsetTime { get; set; }

    [Name("Rise.Time")] public required string RiseTime { get; set; }

    [Name("Total.Elapsed.Bed.Time")] public required string TotalElapsedBedTime { get; set; }

    [Name("Total.Sleep.Time")] public required string TotalSleepTime { get; set; }

    [Name("Total.Wake.Time")] public required string TotalWakeTime { get; set; }

    [Name("Sleep.Efficiency")] public required string SleepEfficiency { get; set; }

    [Name("Num.Active.Periods")] public required string NumActivePeriods { get; set; }

    [Name("Median.Activity.Length")] public required string MedianActivityLength { get; set; }
}