using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    
    [ObservableProperty]
    private ObservableCollection<Analysis> _selectedAnalyses = new();
    
    public SleepPageViewModel(SharedDataService sharedDataService)
    {
        _sharedDataService = sharedDataService;
        
        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        
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