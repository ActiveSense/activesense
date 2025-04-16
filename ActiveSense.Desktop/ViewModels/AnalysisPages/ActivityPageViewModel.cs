using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
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
    
    private ChartFactory _chartFactory;
    
    public ObservableCollection<ChartViewModel> ChartViewModels { get; }
    public ActivityPageViewModel(SharedDataService sharedDataService, ChartFactory chartFactory)
    {
        _chartFactory = chartFactory;
        
        _sharedDataService = sharedDataService;
        
        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        
        // Load initial data
        UpdateSelectedAnalyses();
        
        ChartViewModels = new ObservableCollection<ChartViewModel>
        {
            chartFactory.GetChartViewModel(ChartTypes.Steps),
        };
        
    }
    
    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        foreach (var chartViewModel in ChartViewModels)
        {
            chartViewModel.UpdateChartData(SelectedAnalyses);
        }
    }
    
    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses)
        {
            SelectedAnalyses.Add(analysis);
        }
    }
}