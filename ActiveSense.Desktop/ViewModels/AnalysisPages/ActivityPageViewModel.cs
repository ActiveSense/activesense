using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class ActivityPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    
    [ObservableProperty]
    private ObservableCollection<Analysis> _selectedAnalyses = new();
    
    public ObservableCollection<ChartViewModel> ChartViewModels { get; }
    public ActivityPageViewModel(SharedDataService sharedDataService)
    {
        _sharedDataService = sharedDataService;
        
        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        
        // Load initial data
        UpdateSelectedAnalyses();
        
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
    }
}