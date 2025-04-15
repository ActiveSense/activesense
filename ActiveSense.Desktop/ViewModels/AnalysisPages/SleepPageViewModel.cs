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
using SkiaSharp;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
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
    
    public SleepPageViewModel(SharedDataService sharedDataService)
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
        if (SelectedAnalyses.Count == 0 || !Enumerable.Any<Analysis>(SelectedAnalyses, a => a.SleepRecords.Count > 0))
        {
            InitializeEmptyChart();
            return;
        }
        
        // Process sleep records to extract bed times
        var sleepRecords = Enumerable
            .SelectMany<Analysis, SleepRecord>(SelectedAnalyses, a => a.SleepRecords)
            .ToList();
        
        if (sleepRecords.Count == 0)
        {
            InitializeEmptyChart();
            return;
        }
        
        // Parse the TotalElapsedBedTime values (assuming they're in format "XX.XX hours")
        var bedTimeValues = sleepRecords
            .Select(r => {
                double.TryParse(r.TotalElapsedBedTime.Replace(" hours", "").Trim(), out var time);
                return time;
            })
            .ToArray();
        
        // Get the night dates for labels
        var nightDates = sleepRecords
            .Select(r => r.NightStarting)
            .ToArray();
        
        // Create background series (maximum value for reference)
        var maxValue = Math.Ceiling(bedTimeValues.Max());
        var backgroundValues = Enumerable.Repeat(maxValue, bedTimeValues.Length).ToArray();
        
        // Calculate mean for bed time values
        double meanBedTime = bedTimeValues.Average();
        var meanValues = Enumerable.Repeat(meanBedTime, bedTimeValues.Length).ToArray();
        
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
                Values = bedTimeValues,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                IgnoresBarPosition = true,
                Name = "Total Elapsed Bed Time"
            },
            new LineSeries<double>
            {
                Values = meanValues,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                GeometrySize = 0,
                Name = "Mean Bed Time",
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