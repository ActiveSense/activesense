using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Core.Domain.Models;

public class GeneActiveAnalysis(DateToWeekdayConverter dateToWeekdayConverter)
    : IActivityAnalysis, ISleepAnalysis, IChartDataProvider
{
    private const string DateFormat = "dd.MM.yyyy";


    private readonly Dictionary<string, object> _cache = new();
    private List<ActivityRecord> _activityRecords = [];
    private List<SleepRecord> _sleepRecords = [];
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool Exported { get; set; } = false;


    #region Collections

    public IReadOnlyCollection<ActivityRecord> ActivityRecords => _activityRecords.AsReadOnly();
    public IReadOnlyCollection<SleepRecord> SleepRecords => _sleepRecords.AsReadOnly();

    public List<AnalysisTag> Tags { get; set; } = [];

    public void AddTag(string name, string color)
    {
        Tags.Add(new AnalysisTag(name, color));
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

    public double[] ActivePeriods => GetCachedValue(() =>
        _sleepRecords
            .Select(record => TryParseDouble(record.NumActivePeriods))
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

    public string[] SleepWeekdays()
    {
        return GetCachedValue(() =>
        {
            var weekdays = _sleepRecords
                .Select(r => dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting))
                .ToArray();
            return GetUniqueWeekdayLabels(weekdays);
        });
    }

    public string[] ActivityWeekdays()
    {
        return GetCachedValue(() =>
        {
            var weekdays = _activityRecords
                .Select(r => dateToWeekdayConverter.ConvertDateToWeekday(r.Day))
                .ToArray();
            return GetUniqueWeekdayLabels(weekdays);
        });
    }


    public string[] ActivityDates()
    {
        return GetCachedValue(() =>
        {
            var dates = _activityRecords
                .Select(r => ParseAndFormatDate(r.Day, DateFormat))
                .ToArray();
            return dates;
        });
    }

    public string[] SleepDates()
    {
        return GetCachedValue(() =>
        {
            var dates = _sleepRecords
                .Select(r => ParseAndFormatDate(r.NightStarting, DateFormat))
                .ToArray();
            return dates;
        });
    }

    #endregion

    #region Chart Data

    public IEnumerable<ChartDataDTO> GetActivityDistributionChartData()
    {
        var dates = ActivityWeekdays();

        return new List<ChartDataDTO>
        {
            new()
            {
                Data = LightActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
                Labels = dates,
                Title = "Leichte Aktivität"
            },
            new()
            {
                Data = ModerateActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
                Labels = dates,
                Title = "Mittlere Aktivität"
            },
            new()
            {
                Data = VigorousActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
                Labels = dates,
                Title = "Intensive Aktivität"
            }
        };
    }

    public ChartDataDTO GetSleepDistributionChartData()
    {
        return new ChartDataDTO
        {
            Labels = new[] { "Zeit Schlafend", "Zeit Wach" },
            Data = new[] { Math.Round(AverageSleepTime / 3600, 1), Math.Round(AverageWakeTime / 3600, 1) },
            Title = $"{FileName}"
        };
    }

    public ChartDataDTO GetSleepEfficiencyChartData()
    {
        return new ChartDataDTO
        {
            Data = SleepEfficiency,
            Labels = SleepWeekdays(),
            Title = $"{FileName} ({this.GetSleepDateRange()})"
        };
    }

    public ChartDataDTO GetActivePeriodsChartData()
    {
        return new ChartDataDTO
        {
            Data = ActivePeriods,
            Labels = SleepWeekdays(),
            Title = $"{FileName} ({this.GetSleepDateRange()})"
        };
    }

    public ChartDataDTO GetMovementPatternChartData()
    {
        return new ChartDataDTO
        {
            Labels = new[] { "Aktivität", "Schlaf", "Sitzzeit" },
            Data = new[]
            {
                Math.Round((AverageLightActivity + AverageModerateActivity + AverageVigorousActivity) / 3600, 2), Math.Round(AverageSleepTime / 3600, 2) , Math.Round(AverageSedentaryTime / 3600, 2) 
            },
            Title = $"{FileName}"
        };
    }

    public ChartDataDTO GetTotalSleepTimePerDayChartData()
    {
        return new ChartDataDTO
        {
            Data = TotalSleepTimePerDay.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
            Labels = SleepWeekdays(),
            Title = $"{FileName} ({this.GetSleepDateRange()})"
        };
    }

    public ChartDataDTO GetStepsChartData()
    {
        return new ChartDataDTO
        {
            Data = StepsPerDay,
            Labels = ActivityWeekdays(),
            Title = $"{FileName} ({this.GetActivityDateRange()})"
        };
    }

    public ChartDataDTO GetSedentaryChartData()
    {
        return new ChartDataDTO
        {
            Data = SedentaryActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
            Labels = ActivityWeekdays(),
            Title = $"{FileName} ({this.GetActivityDateRange()})"
        };
    }

    public ChartDataDTO GetLightActivityChartData()
    {
        return new ChartDataDTO
        {
            Data = LightActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
            Labels = ActivityWeekdays(),
            Title = $"{FileName} ({this.GetActivityDateRange()})"
        };
    }

    public ChartDataDTO GetModerateActivityChartData()
    {
        return new ChartDataDTO
        {
            Data = ModerateActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
            Labels = ActivityWeekdays(),
            Title = $"{FileName} ({this.GetActivityDateRange()})"
        };
    }

    public ChartDataDTO GetVigorousActivityChartData()
    {
        return new ChartDataDTO
        {
            Data = VigorousActivity.Select(seconds => Math.Round(seconds / 3600, 1)).ToArray(),
            Labels = ActivityWeekdays(),
            Title = $"{FileName} ({this.GetActivityDateRange()})"
        };
    }

    #endregion

    #region Helper Methods

    private void ClearCache()
    {
        _cache.Clear();
    }

    private T GetCachedValue<T>(Func<T> valueFactory, [CallerMemberName] string key = null!)
    {
        if (_cache.TryGetValue(key, out var value) && value is T cachedValue) return cachedValue;

        var newValue = valueFactory();
        _cache[key] = newValue!;
        return newValue;
    }

    private static double TryParseDouble(string value)
    {
        return double.TryParse(value, out var result) ? result : 0;
    }

    private string ParseAndFormatDate(string dateString, string format)
    {
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date.ToString(format);

        return dateString;
    }

    private string[] GetUniqueWeekdayLabels(string[] weekdays)
    {
        if (weekdays == null || weekdays.Length == 0)
            return [];

        var uniqueLabels = new string[weekdays.Length];
        var weekdayCounts = new Dictionary<string, int>();

        for (var i = 0; i < weekdays.Length; i++)
        {
            var weekday = weekdays[i];

            if (weekdayCounts.TryAdd(weekday, 1))
            {
                uniqueLabels[i] = weekday;
            }
            else
            {
                var count = ++weekdayCounts[weekday];
                uniqueLabels[i] = $"{count}. {weekday} ";
            }
        }

        return uniqueLabels;
    }

    #endregion
}