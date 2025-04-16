using System;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class SleepDurationChartViewModel : ChartViewModel
{
    private readonly DateToWeekdayConverter _dateToWeekdayConverter;

    [ObservableProperty] private string[] _labels;

    [ObservableProperty] private ISeries[] _series;

    [ObservableProperty] private ICartesianAxis[] _xAxes;

    [ObservableProperty] private ICartesianAxis[] _yAxes;

    public SleepDurationChartViewModel(DateToWeekdayConverter dateToWeekdayConverter)
    {
        Title = "Sleep Duration";
        Description = "Shows sleep duration across nights";
        _dateToWeekdayConverter = dateToWeekdayConverter;
    }

    public override void UpdateChartData(ObservableCollection<Analysis> analyses)
    {
        // Extract all sleep records and convert to necessary data
        var sleepRecords = analyses.SelectMany(a => a.SleepRecords).ToList();
    
        var sleepTime = sleepRecords
            .Select(r => double.TryParse(r.TotalSleepTime, out var time) ? time / 3600 : 0)
            .ToArray();
    
        var weekDaysLabel = sleepRecords
            .Select(r => _dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting))
            .ToArray();
    
        // Calculate values for chart display
        var maxValue = Math.Ceiling(sleepTime.Any() ? sleepTime.Max() : 0);
        var meanSleep = sleepTime.Any() ? sleepTime.Average() : 0;
    
        // Set up series
        Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = sleepTime,
                Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                Name = "Schlafzeit"
            },
            new LineSeries<double>
            {
                Values = Enumerable.Repeat(meanSleep, sleepTime.Length).ToArray(),
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Name = "Durchschnitt",
                LineSmoothness = 0
            }
        };
    
        // Set up axes
        YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = maxValue + 1 } };
        XAxes = new[] { new Axis { Labels = weekDaysLabel, LabelsRotation = -45 } };
        Labels = weekDaysLabel;
    }
}