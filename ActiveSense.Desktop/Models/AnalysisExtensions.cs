using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Models;

public static class AnalysisExtensions
{
    /// <summary>
    ///     Returns the date range of the sleep dataset as a formatted string.
    /// </summary>
    /// <param name="analysis">The sleep analysis object</param>
    /// <param name="format">Optional date format (default: "dd.MM.yyyy")</param>
    /// <returns>A string representing the date range (e.g., "01.01.2023 - 07.01.2023")</returns>
    public static string GetSleepDateRange(this ISleepAnalysis analysis, string format = "dd.MM.yyyy")
    {
        if (analysis == null || analysis.SleepRecords == null || !analysis.SleepRecords.Any())
            return "No sleep data available";

        try
        {
            var dates = new List<DateTime>();

            // Parse all dates from sleep records
            foreach (var record in analysis.SleepRecords)
                if (DateTime.TryParse(record.NightStarting, out var date))
                    dates.Add(date);

            if (!dates.Any()) return "No valid dates found in sleep data";

            // Find min and max dates
            var startDate = dates.Min();
            var endDate = dates.Max();

            // Format the date range string
            return $"{startDate.ToString(format)} - {endDate.ToString(format)}";
        }
        catch (Exception ex)
        {
            // In case of any parsing errors
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

            // Parse all dates from activity records
            foreach (var record in analysis.ActivityRecords)
                if (DateTime.TryParse(record.Day, out var date))
                    dates.Add(date);

            if (!dates.Any()) return "No valid dates found in activity data";

            // Find min and max dates
            var startDate = dates.Min();
            var endDate = dates.Max();

            // Format the date range string
            return $"{startDate.ToString(format)} - {endDate.ToString(format)}";
        }
        catch (Exception ex)
        {
            // In case of any parsing errors
            return $"Error calculating date range: {ex.Message}";
        }
    }
}