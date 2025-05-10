using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Core.Domain.Models;

public static class GeneActiveAnalysisExtensions
{
    public static string GetSleepDateRange(this ISleepAnalysis analysis, string format = "dd.MM.yyyy")
    {
        if (analysis == null || analysis.SleepRecords == null || !analysis.SleepRecords.Any())
            return "No sleep data available";

        try
        {
            var dates = new List<DateTime>();

            foreach (var record in analysis.SleepRecords)
                if (DateTime.TryParse(record.NightStarting, out var date))
                    dates.Add(date);

            if (!dates.Any()) return "No valid dates found in sleep data";

            var startDate = dates.Min();
            var endDate = dates.Max();

            return $"{startDate.ToString(format)} - {endDate.ToString(format)}";
        }
        catch (Exception ex)
        {
            return $"Error calculating date range: {ex.Message}";
        }
    }

    public static string GetActivityDateRange(this IActivityAnalysis analysis, string format = "dd.MM.yyyy")
    {
        if (analysis == null || analysis.ActivityRecords == null || !analysis.ActivityRecords.Any())
            return "No activity data available";

        try
        {
            var dates = new List<DateTime>();

            foreach (var record in analysis.ActivityRecords)
                if (DateTime.TryParse(record.Day, out var date))
                    dates.Add(date);

            if (!dates.Any()) return "No valid dates found in activity data";

            var startDate = dates.Min();
            var endDate = dates.Max();

            return $"{startDate.ToString(format)} - {endDate.ToString(format)}";
        }
        catch (Exception ex)
        {
            return $"Error calculating date range: {ex.Message}";
        }
    }
}