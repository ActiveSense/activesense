using System;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class StepsChartViewModel : ChartViewModel
{
    [ObservableProperty] private string[] _labels;
    [ObservableProperty] private ISeries[] _series;

    [ObservableProperty] private ICartesianAxis[] _xAxes;

    [ObservableProperty] private ICartesianAxis[] _yAxes;

    public StepsChartViewModel()
    {
        Title = "Schritte";
        Description = "Shows sleep duration across nights";
    }

    public override void UpdateChartData(ObservableCollection<Analysis> analyses)
    {
        var analysesCount = analyses.Count;
        // Process activity records to extract step counts
        var activityRecords = analyses
            .SelectMany<Analysis, ActivityRecord>(a => a.ActivityRecords)
            .ToList();

        // Parse the Steps values
        var stepValues = activityRecords
            .Select(r =>
            {
                int.TryParse(r.Steps, out var steps);
                return (double)steps;
            })
            .ToArray();

        // Get the day numbers for labels
        var dayLabels = activityRecords
            .Select(r => $"Day {r.Day}")
            .ToArray();

        // Create background series (maximum value for reference)
        var maxValue = Math.Ceiling(stepValues.Max());
        var backgroundValues = Enumerable.Repeat(maxValue, stepValues.Length).ToArray();

        // Calculate mean for step values
        var meanSteps = stepValues.Average();
        var meanValues = Enumerable.Repeat(meanSteps, stepValues.Length).ToArray();

        // Set up the chart
        Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                IsHoverable = false,
                Values = backgroundValues,
                Stroke = null,
                Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                IgnoresBarPosition = true
            },
            new ColumnSeries<double>
            {
                Values = stepValues,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.LimeGreen),
                IgnoresBarPosition = true,
                Name = "Steps"
            },
            new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Mean Steps",
                LineSmoothness = 0 // Straight line
            }
        };

        YAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = maxValue + 1000 // Add some padding
            }
        };

        XAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = dayLabels,
                LabelsRotation = -45
            }
        };

        Labels = dayLabels;
    }
}