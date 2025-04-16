using System;
using System.Collections.Generic;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Models;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.Sensors;

public class BarChartGenerator
{
    private readonly DateToWeekdayConverter _dateToWeekdayConverter;

    public BarChartGenerator(DateToWeekdayConverter dateToWeekdayConverter)
    {
        _dateToWeekdayConverter = dateToWeekdayConverter;
    }

    public (ISeries[] Series, ICartesianAxis[] XAxes, ICartesianAxis[] YAxes) GetSleepTimeChart(
        IEnumerable<Analysis> analyses)
    {
        // Handle empty input
        if (!analyses.Any() || analyses.All(a => a.SleepRecords.Count == 0))
            return (Array.Empty<ISeries>(), Array.Empty<ICartesianAxis>(), Array.Empty<ICartesianAxis>());

        // Get all unique dates and their weekday labels
        var allSleepRecords = analyses.SelectMany(a => a.SleepRecords).ToList();
        var allDates = allSleepRecords
            .Select(r => new DateInfo(r.NightStarting, _dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting)))
            .Distinct(new DateInfoComparer())
            .OrderBy(d => d.OriginalDate)
            .ToList();

        var weekDayLabels = allDates.Select(d => d.WeekDay).ToArray();

        // Create series for each analysis
        var seriesList = new List<ISeries>();
        var colors = GetColorPalette(analyses.Count());
        var colorIndex = 0;

        // Calculate average sleep time by weekday
        var sleepTimesByWeekday = new Dictionary<string, List<double>>();
        foreach (var date in allDates) sleepTimesByWeekday[date.WeekDay] = new List<double>();

        foreach (var analysis in analyses)
        {
            // Create a dictionary to map weekdays to sleep time values
            var sleepByWeekday = new Dictionary<string, double>();
            foreach (var date in allDates) sleepByWeekday[date.WeekDay] = 0;

            // Fill in actual values where they exist
            foreach (var record in analysis.SleepRecords)
            {
                var weekday = _dateToWeekdayConverter.ConvertDateToWeekday(record.NightStarting);
                if (double.TryParse(record.TotalSleepTime, out var timeValue))
                {
                    var timeInHours = timeValue / 3600;
                    sleepByWeekday[weekday] = timeInHours;

                    // Add to average calculation
                    sleepTimesByWeekday[weekday].Add(timeInHours);
                }
            }

            // Extract values in the same order as the weekday labels
            var normalizedValues = weekDayLabels
                .Select(day => sleepByWeekday[day])
                .ToArray();

            // Add series for this analysis
            seriesList.Add(new ColumnSeries<double>
            {
                Values = normalizedValues,
                Stroke = null,
                Fill = new SolidColorPaint(colors[colorIndex++]),
                MaxBarWidth = 15,
                Name = analysis.FileName
            });
        }

// Calculate and add average line
        if (analyses.Count() > 1)
        {
            // Multiple analyses: Calculate average for each weekday separately
            var averages = weekDayLabels
                .Select(day => sleepTimesByWeekday[day].Count > 0
                    ? sleepTimesByWeekday[day].Average()
                    : 0)
                .ToArray();

            // Add day-by-day average line
            seriesList.Add(new LineSeries<double>
            {
                Values = averages,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Täglicher Durchschnitt",
                LineSmoothness = 0
            });
        }
        else
        {
            // Single analysis: Calculate one average across all days
            double totalAverage = 0;
            var allSleepTimes = sleepTimesByWeekday.Values
                .SelectMany(times => times)
                .ToList();

            if (allSleepTimes.Count > 0) totalAverage = allSleepTimes.Average();

            // Create flat average line with the same value across all days
            var averageValues = Enumerable.Repeat(totalAverage, weekDayLabels.Length).ToArray();

            // Add flat average line
            seriesList.Add(new LineSeries<double>
            {
                Values = averageValues,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Wochendurchschnitt",
                LineSmoothness = 0
            });
        }

        // Create X axis
        var xAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = weekDayLabels,
                LabelsRotation = -45
            }
        };

        // Calculate Y-axis scale
        var allTimes = allSleepRecords
            .Select(r =>
            {
                double.TryParse(r.TotalSleepTime, out var time);
                return time / 3600;
            });

        var maxValue = allTimes.Any() ? Math.Ceiling(allTimes.Max()) : 10;

        // Create Y axis
        var yAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Name = "Hours",
                MinLimit = 0,
                MaxLimit = maxValue + 1
            }
        };

        return (seriesList.ToArray(), xAxes, yAxes);
    }

    // Generate a palette of colors for the different analyses
    private SKColor[] GetColorPalette(int count)
    {
        var predefinedColors = new[]
        {
            SKColors.CornflowerBlue,
            SKColors.Orange,
            SKColors.ForestGreen,
            SKColors.Crimson,
            SKColors.Purple,
            SKColors.Gold,
            SKColors.Teal,
            SKColors.DarkSlateBlue
        };

        if (count <= predefinedColors.Length) return predefinedColors.Take(count).ToArray();

        // Generate additional colors for larger datasets
        var colors = new SKColor[count];
        for (var i = 0; i < count; i++)
            if (i < predefinedColors.Length)
            {
                colors[i] = predefinedColors[i];
            }
            else
            {
                var hue = 360f / (count - predefinedColors.Length) * (i - predefinedColors.Length);
                colors[i] = SKColor.FromHsl(hue, 80, 60);
            }

        return colors;
    }

    // Helper class to store date information
    private class DateInfo
    {
        public DateInfo(string originalDate, string weekDay)
        {
            OriginalDate = originalDate;
            WeekDay = weekDay;
        }

        public string OriginalDate { get; }
        public string WeekDay { get; }
    }

    // Custom comparer for DateInfo that compares by weekday
    private class DateInfoComparer : IEqualityComparer<DateInfo>
    {
        public bool Equals(DateInfo x, DateInfo y)
        {
            if (x == null || y == null)
                return false;

            return x.WeekDay == y.WeekDay;
        }

        public int GetHashCode(DateInfo obj)
        {
            return obj.WeekDay.GetHashCode();
        }
    }
}

public class PieChartGenerator
{
    public ISeries[] GetPieChartForAnalysis(Analysis analysis)
    {
        // Extract sleep data from this specific analysis
        var totalSleepTime = 0.0;
        var totalWakeTime = 0.0;
        
        foreach (var record in analysis.SleepRecords)
        {
            double.TryParse(record.TotalSleepTime, out var sleepTime);
            double.TryParse(record.TotalWakeTime, out var wakeTime);
            
            totalSleepTime += sleepTime;
            totalWakeTime += wakeTime;
        }
        
        // Create pie chart series for sleep/wake distribution
        return new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new[] { totalSleepTime },
                Name = "Sleep Time",
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            },
            new PieSeries<double>
            {
                Values = new[] { totalWakeTime },
                Name = "Wake Time",
                Fill = new SolidColorPaint(SKColors.Orange)
            }
        };
    }
}