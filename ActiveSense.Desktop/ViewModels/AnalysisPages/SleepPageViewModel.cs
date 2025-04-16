using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel;
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
    
    public ObservableCollection<ChartViewModel> ChartViewModels { get; }
    
    public SleepPageViewModel(SharedDataService sharedDataService, ChartFactory chartFactory)
    {
        _sharedDataService = sharedDataService;
        
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        
        UpdateSelectedAnalyses();
        
        ChartViewModels = new ObservableCollection<ChartViewModel>
        {
            chartFactory.GetChartViewModel(ChartTypes.SleepEfficiency),
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