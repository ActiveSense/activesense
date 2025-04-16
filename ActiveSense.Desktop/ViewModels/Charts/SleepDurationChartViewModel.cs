using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class SleepDurationChartViewModel : ChartViewModel
{
    [ObservableProperty] private ISeries[] _series;

    [ObservableProperty] private ICartesianAxis[] _xAxes;

    [ObservableProperty] private ICartesianAxis[] _yAxes;
    
    [ObservableProperty] private string[] _labels;

    public SleepDurationChartViewModel()
    {
        Title = "Sleep Duration";
        Description = "Shows sleep duration across nights";
    }

    public override void UpdateChartData(ObservableCollection<Analysis> analyses)
    {
        var analysesCount = analyses.Count;
        
        var sleepRecords = analyses
            .SelectMany(a => a.SleepRecords)
            .ToList();

        var sleepTime = sleepRecords
            .Select(r =>
            {
                if (double.TryParse(r.TotalSleepTime, out var time))
                {
                    return time / 3600;
                }
                return 0;
            })
            .ToArray();

        var nightDates = sleepRecords
            .Select(r => r.NightStarting)
            .ToArray();

        var maxValue = Math.Ceiling(sleepTime.Max());
        var backgroundValues = Enumerable.Repeat(maxValue, sleepTime.Length).ToArray();
        
        double meanSleep = sleepTime.Average();
        var meanValues = Enumerable.Repeat(meanSleep, sleepTime.Length).ToArray();

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
                Values = sleepTime,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                IgnoresBarPosition = true,
                Name = "Schlafzeit"
            },
            new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Durchschnitt",
                LineSmoothness = 0 // Straight line
            }
        };

        YAxes = new ICartesianAxis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = maxValue + 1, // Add some padding
            }
        };

        XAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = nightDates,
                LabelsRotation = -45
            }
        };

        Labels = nightDates;
    }
}