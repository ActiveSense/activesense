using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Converters;
using CsvHelper.Configuration.Attributes;

namespace ActiveSense.Desktop.Models;

public class Analysis(DateToWeekdayConverter dateToWeekdayConverter)
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
    
    public double[] StepsPerDay => ActivityRecords
        .Select(record => double.TryParse(record.Steps, out var steps) ? steps : 0)
        .ToArray();
    
    public string[] Days => ActivityRecords
        .Select(record => record.Day)
        .ToArray();
    
    public double[] ModerateActivity => ActivityRecords
        .Select(record => double.TryParse(record.Moderate, out var steps) ? steps : 0)
        .ToArray();
    public double[] VigorousActivity => ActivityRecords
        .Select(record => double.TryParse(record.Vigorous, out var steps) ? steps : 0)
        .ToArray();
    public double[] SedentaryActivity => ActivityRecords
        .Select(record => double.TryParse(record.Sedentary, out var steps) ? steps : 0)
        .ToArray();
    public double[] LightActivity => ActivityRecords
        .Select(record => double.TryParse(record.Light, out var steps) ? steps : 0)
        .ToArray();
    
    public string[] SleepWeekdays()
    {
      var weekdays = SleepRecords 
              .Select(r => dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting))
              .ToArray();
      return GetUniqueWeekdayLabels(weekdays);
    } 

    public string[] ActivityWeekdays()
    {
      var weekdays = ActivityRecords 
              .Select(r => dateToWeekdayConverter.ConvertDateToWeekday(r.Day))
              .ToArray();
      return GetUniqueWeekdayLabels(weekdays);
    } 
    private string[] GetUniqueWeekdayLabels(string[] weekdays)
        {
            if (weekdays == null || weekdays.Length == 0)
                return Array.Empty<string>();
                
            var uniqueLabels = new string[weekdays.Length];
            var weekdayCounts = new Dictionary<string, int>();
            
            for (int i = 0; i < weekdays.Length; i++)
            {
                string weekday = weekdays[i];
                
                // If this is the first time we've seen this weekday, just use it
                if (!weekdayCounts.ContainsKey(weekday))
                {
                    weekdayCounts[weekday] = 1;
                    uniqueLabels[i] = weekday;
                }
                else
                {
                    // This is a duplicate, so add the occurrence count
                    int count = ++weekdayCounts[weekday];
                    uniqueLabels[i] = $"{weekday} {count}";
                }
            }
            
            return uniqueLabels;
        } 
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