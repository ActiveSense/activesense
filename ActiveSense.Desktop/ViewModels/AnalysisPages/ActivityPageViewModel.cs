using System;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class ActivityPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    
    [ObservableProperty]
    private ObservableCollection<Analysis> _selectedAnalyses = new();
    
    [ObservableProperty]
    private ISeries[] _series;
    
    [ObservableProperty]
    private ICartesianAxis[] _yAxes;
    
    [ObservableProperty]
    private ICartesianAxis[] _xAxes;
    
    [ObservableProperty]
    private string[] _labels;
    
    public ActivityPageViewModel(SharedDataService sharedDataService)
    {
        _sharedDataService = sharedDataService;
        
        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        
        // Initialize empty chart
        InitializeEmptyChart();
        
        // Load initial data
        UpdateSelectedAnalyses();
    }
    
    private void InitializeEmptyChart()
    {
        // Default empty chart setup
        Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                IsHoverable = false,
                Values = new double[] { 0 },
                Stroke = null,
                Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                IgnoresBarPosition = true
            }
        };
        
        YAxes = new ICartesianAxis[]
        {
            new Axis 
            { 
                MinLimit = 0,
                MaxLimit = 10,
            }
        };

        XAxes = new ICartesianAxis[]
        {
            new Axis
            {
                Labels = new[] { "No data" },
                LabelsRotation = -45
            }
        };
        
        Labels = new[] { "No data" };
    }
    
    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
    }
    
    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses)
        {
            SelectedAnalyses.Add(analysis);
        }
        
        UpdateChart();
    }
    
    private void UpdateChart()
    {
        if (SelectedAnalyses.Count == 0 || !Enumerable.Any<Analysis>(SelectedAnalyses, a => a.ActivityRecords.Count > 0))
        {
            InitializeEmptyChart();
            return;
        }
        
        // Process activity records to extract step counts
        var activityRecords = Enumerable
            .SelectMany<Analysis, ActivityRecord>(SelectedAnalyses, a => a.ActivityRecords)
            .ToList();
        
        if (activityRecords.Count == 0)
        {
            InitializeEmptyChart();
            return;
        }
        
        // Parse the Steps values
        var stepValues = activityRecords
            .Select(r => {
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
        double meanSteps = stepValues.Average();
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
                MaxLimit = maxValue + 1000, // Add some padding
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