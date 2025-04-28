using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Charts.DTOs;
using CsvHelper.Configuration.Attributes;

namespace ActiveSense.Desktop.Models;

public class Analysis
{
    private readonly DateToWeekdayConverter _dateToWeekdayConverter;
    private readonly Dictionary<string, object> _cache = new();

    private List<ActivityRecord> _activityRecords = new();
    private List<SleepRecord> _sleepRecords = new();
    
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public bool Exported { get; set; } = false;

    public Analysis(DateToWeekdayConverter dateToWeekdayConverter)
    {
        _dateToWeekdayConverter = dateToWeekdayConverter;
    }

    #region Collections

    public IReadOnlyCollection<ActivityRecord> ActivityRecords => _activityRecords.AsReadOnly();
    public IReadOnlyCollection<SleepRecord> SleepRecords => _sleepRecords.AsReadOnly();

    public void AddActivityRecords(IEnumerable<ActivityRecord> records)
    {
        _activityRecords.AddRange(records);
        ClearCache();
    }

    public void AddSleepRecords(IEnumerable<SleepRecord> records)
    {
        _sleepRecords.AddRange(records);
        ClearCache();
    }

    public void SetActivityRecords(IEnumerable<ActivityRecord> records)
    {
        _activityRecords = records.ToList();
        ClearCache();
    }

    public void SetSleepRecords(IEnumerable<SleepRecord> records)
    {
        _sleepRecords = records.ToList();
        ClearCache();
    }

    #endregion

    #region Sleep Metrics

    public double TotalSleepTime => GetCachedValue(() => 
        _sleepRecords.Sum(record => TryParseDouble(record.TotalSleepTime)));

    public double TotalWakeTime => GetCachedValue(() => 
        _sleepRecords.Sum(record => TryParseDouble(record.TotalWakeTime)));

    public double AverageSleepTime => GetCachedValue(() => 
        _sleepRecords.Any() 
            ? _sleepRecords.Average(record => TryParseDouble(record.TotalSleepTime)) 
            : 0);

    public double AverageWakeTime => GetCachedValue(() => 
        _sleepRecords.Any() 
            ? _sleepRecords.Average(record => TryParseDouble(record.TotalWakeTime)) 
            : 0);

    
    public double[] SleepEfficiency => GetCachedValue(() => 
        _sleepRecords
            .Select(record => TryParseDouble(record.SleepEfficiency))
            .ToArray());
    
    public double[] TotalSleepTimePerDay => GetCachedValue(() => 
        _sleepRecords
            .Select(record => TryParseDouble(record.TotalSleepTime))
            .ToArray());

    #endregion

    #region Activity Metrics

    public double[] StepsPerDay => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Steps))
            .ToArray());

    public string[] Days => GetCachedValue(() => 
        _activityRecords
            .Select(record => record.Day)
            .ToArray());

    public double[] ModerateActivity => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Moderate))
            .ToArray());

    public double[] VigorousActivity => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Vigorous))
            .ToArray());

    public double[] SedentaryActivity => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Sedentary))
            .ToArray());

    public double[] LightActivity => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Light))
            .ToArray());

    public double[] StepsPercentage => GetCachedValue(() => 
        _activityRecords
            .Select(record => TryParseDouble(record.Steps))
            .Select(steps => steps / 10000 * 100)
            .ToArray());

    public double AverageSedentaryTime => GetCachedValue(() => 
        _activityRecords.Any() 
            ? _activityRecords.Average(record => TryParseDouble(record.Sedentary)) 
            : 0);
    
    public double AverageLightActivity => GetCachedValue(() => 
        _activityRecords.Any() 
            ? _activityRecords.Average(record => TryParseDouble(record.Light)) 
            : 0);
    public double AverageModerateActivity => GetCachedValue(() => 
        _activityRecords.Any() 
            ? _activityRecords.Average(record => TryParseDouble(record.Moderate)) 
            : 0);
    
    public double AverageVigorousActivity => GetCachedValue(() => 
        _activityRecords.Any() 
            ? _activityRecords.Average(record => TryParseDouble(record.Vigorous)) 
            : 0);
    #endregion

    #region Data Access Methods

    public string[] SleepWeekdays() => GetCachedValue(() =>
    {
        var weekdays = _sleepRecords
            .Select(r => _dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting))
            .ToArray();
        return GetUniqueWeekdayLabels(weekdays);
    }, "SleepWeekdays");

    public string[] ActivityWeekdays() => GetCachedValue(() =>
    {
        var weekdays = _activityRecords
            .Select(r => _dateToWeekdayConverter.ConvertDateToWeekday(r.Day))
            .ToArray();
        return GetUniqueWeekdayLabels(weekdays);
    }, "ActivityWeekdays");

    #endregion

    #region Chart Data

    public IEnumerable<ChartDataDTO> GetActivityDistributionChartData()
    {
        var weekdays = ActivityWeekdays();
        
        return new List<ChartDataDTO>
        {
            new ChartDataDTO
            {
                Data = LightActivity,
                Labels = weekdays,
                Title = "Leichte Aktivität"
            },
            new ChartDataDTO
            {
                Data = ModerateActivity,
                Labels = weekdays,
                Title = "Mittlere Aktivität"
            },
            new ChartDataDTO
            {
                Data = VigorousActivity,
                Labels = weekdays,
                Title = "Intensive Aktivität"
            }
        };
    }

    public ChartDataDTO GetSleepChartData()
    {
        return new ChartDataDTO
        {
            Labels = new[] { "Total Sleep Time", "Total Wake Time" },
            Data = new[] { TotalSleepTime, TotalWakeTime },
            Title = $"Schlafverteilung {FileName}"
        };
    }

    public ChartDataDTO GetMovementPatternChartData()
    {
        return new ChartDataDTO()
        {
            Labels = new[] { "Aktivität", "Schlaf", "Sitzzeit" },
            Data = new[] { (AverageLightActivity + AverageModerateActivity + AverageVigorousActivity), AverageSleepTime, AverageSedentaryTime },
            Title = $"Aktivitätsverteilung {FileName}",
        };
    }
    
    public ChartDataDTO GetTotalSleepTimePerDayChartData()
    {
        return new ChartDataDTO
        {
            Data = TotalSleepTimePerDay,
            Labels = SleepWeekdays(),
            Title = FileName
        };
    }

    public ChartDataDTO GetStepsChartData()
    {
        return new ChartDataDTO
        {
            Data = StepsPerDay,
            Labels = ActivityWeekdays(),
            Title = FileName
        };
    }

    #endregion

    #region Helper Methods

    private void ClearCache()
    {
        _cache.Clear();
    }

    private T GetCachedValue<T>(Func<T> valueFactory, [CallerMemberName] string key = null)
    {
        if (_cache.TryGetValue(key, out var value) && value is T cachedValue)
        {
            return cachedValue;
        }

        var newValue = valueFactory();
        _cache[key] = newValue;
        return newValue;
    }

    private static double TryParseDouble(string value)
    {
        return double.TryParse(value, out var result) ? result : 0;
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
            
            if (!weekdayCounts.ContainsKey(weekday))
            {
                weekdayCounts[weekday] = 1;
                uniqueLabels[i] = weekday;
            }
            else
            {
                int count = ++weekdayCounts[weekday];
                uniqueLabels[i] = $"{weekday} {count}";
            }
        }
        
        return uniqueLabels;
    }

    #endregion
}

public class SleepRecord
{
    [Name("Night.Starting")] 
    public string NightStarting { get; set; }

    [Name("Sleep.Onset.Time")] 
    public string SleepOnsetTime { get; set; }

    [Name("Rise.Time")] 
    public string RiseTime { get; set; }

    [Name("Total.Elapsed.Bed.Time")] 
    public string TotalElapsedBedTime { get; set; }

    [Name("Total.Sleep.Time")] 
    public string TotalSleepTime { get; set; }

    [Name("Total.Wake.Time")] 
    public string TotalWakeTime { get; set; }

    [Name("Sleep.Efficiency")] 
    public string SleepEfficiency { get; set; }

    [Name("Num.Active.Periods")] 
    public string NumActivePeriods { get; set; }

    [Name("Median.Activity.Length")] 
    public string MedianActivityLength { get; set; }
}

public class ActivityRecord
{
    [Name("Day.Number")] 
    public string Day { get; set; }

    [Name("Steps")] 
    public string Steps { get; set; }

    [Name("Non_Wear")] 
    public string NonWear { get; set; }

    [Name("Sleep")] 
    public string Sleep { get; set; }

    [Name("Sedentary")] 
    public string Sedentary { get; set; }

    [Name("Light")] 
    public string Light { get; set; }

    [Name("Moderate")] 
    public string Moderate { get; set; }

    [Name("Vigorous")] 
    public string Vigorous { get; set; }
}