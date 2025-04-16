using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ActiveSense.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class ActivityDistributionChartViewModel : ChartViewModel
{
    [ObservableProperty] private ISeries[] _series;
    [ObservableProperty] private ICartesianAxis[] _xAxes;
    [ObservableProperty] private ICartesianAxis[] _yAxes;
    [ObservableProperty] private string[] _labels;
    [ObservableProperty] private string[] _weekdayLabels;
    
    // Pie chart properties
    [ObservableProperty] private ISeries[] _pieSeries;
    [ObservableProperty] private double _averageVigorous;
    [ObservableProperty] private double _averageModerate;
    [ObservableProperty] private double _averageLight;
    [ObservableProperty] private double _totalActivityTime;

    public ActivityDistributionChartViewModel()
    {
        Title = "Activity Distribution";
        Description = "Shows daily activity distribution by intensity level";
    }

    public override void UpdateChartData(ObservableCollection<Analysis> analyses)
    {
        if (analyses == null || analyses.Count == 0 || 
            analyses.All(a => a.ActivityRecords == null || a.ActivityRecords.Count == 0))
        {
            Series = Array.Empty<ISeries>();
            Labels = Array.Empty<string>();
            WeekdayLabels = Array.Empty<string>();
            PieSeries = Array.Empty<ISeries>();
            return;
        }

        // Process activity records
        var activityRecords = analyses
            .SelectMany(a => a.ActivityRecords)
            .ToList();

        // Extract activity values from records
        var vigorousValues = new List<double>();
        var moderateValues = new List<double>();
        var lightValues = new List<double>();

        foreach (var record in activityRecords)
        {
            double.TryParse(record.Vigorous, NumberStyles.Any, CultureInfo.InvariantCulture, out var vigorous);
            double.TryParse(record.Moderate, NumberStyles.Any, CultureInfo.InvariantCulture, out var moderate);
            double.TryParse(record.Light, NumberStyles.Any, CultureInfo.InvariantCulture, out var light);
            
            vigorousValues.Add(vigorous);
            moderateValues.Add(moderate);
            lightValues.Add(light);
        }

        // Get the day labels
        var dayLabels = activityRecords
            .Select(r => $"Day {r.Day}")
            .ToArray();

        // Define the stacked bar series
        Series = new ISeries[]
        {
            new StackedColumnSeries<double>
            {
                Name = "Vigorous",
                Values = vigorousValues,
                Fill = new SolidColorPaint(SKColors.Red),
                Stroke = null,
                DataLabelsSize = 0
            },
            new StackedColumnSeries<double>
            {
                Name = "Moderate",
                Values = moderateValues,
                Fill = new SolidColorPaint(SKColors.Orange),
                Stroke = null,
                DataLabelsSize = 0
            },
            new StackedColumnSeries<double>
            {
                Name = "Light",
                Values = lightValues,
                Fill = new SolidColorPaint(SKColors.LightGreen),
                Stroke = null,
                DataLabelsSize = 0
            }
        };

        // Calculate total activity time for Y-axis scaling
        var totalActivityValues = vigorousValues
            .Zip(moderateValues, (v, m) => v + m)
            .Zip(lightValues, (vm, l) => vm + l)
            .ToArray();
        
        var maxValue = totalActivityValues.Any() ? Math.Ceiling(totalActivityValues.Max()) : 10;

        // Set up the axes
        YAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Name = "Minutes",
                MinLimit = 0,
                MaxLimit = maxValue + (maxValue * 0.1), // Add 10% padding
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
        
        // Calculate totals and summary statistics
        AverageVigorous = vigorousValues.Average();
        AverageModerate = moderateValues.Average();
        AverageLight = lightValues.Average();
        
        PieSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Vigorous",
                Values = new[] { AverageVigorous },
                Fill = new SolidColorPaint(SKColors.Red),
                Stroke = null,
                DataLabelsPosition = PolarLabelsPosition.Middle,
                DataLabelsSize = 12,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{AverageVigorous}",
            },
            new PieSeries<double>
            {
                Name = "Moderate",
                Values = new[] { AverageModerate },
                Fill = new SolidColorPaint(SKColors.Orange),
                Stroke = null,
                DataLabelsPosition = PolarLabelsPosition.Middle,
                DataLabelsSize = 12,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsFormatter = point => $"{AverageModerate}",
            },
            new PieSeries<double>
            {
                Name = "Light",
                Values = new[] { AverageLight },
                Fill = new SolidColorPaint(SKColors.LightGreen),
                Stroke = null,
                DataLabelsPosition = PolarLabelsPosition.Middle,
                DataLabelsSize = 12,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsFormatter = point => $"{AverageLight}",
            }
        };

        Labels = dayLabels;
    }
}