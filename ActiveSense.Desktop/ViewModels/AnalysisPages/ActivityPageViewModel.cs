using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class ActivityPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private bool _chartsVisible = false;
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    
    [ObservableProperty] private string _activityDistributionTitle = "Aktivitätsverteilung";
    [ObservableProperty] private string _activityDistributionDescription = "Aktivitätsverteilung pro Tag";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _activityDistributionChart = new();
    
    [ObservableProperty] private string _stepsTitle = "Schritte pro Tag";
    [ObservableProperty] private string _stepsDescription = "Durchschnittliche Schritte pro Tag";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _stepsCharts = new();

    public ActivityPageViewModel(SharedDataService sharedDataService, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;

        // Load initial data
        UpdateSelectedAnalyses();
        CreateStepsChart();
        CreateActivityDistributionChart();
    }

    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreateStepsChart();
        CreateActivityDistributionChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    #region Chart Generation
    private void CreateActivityDistributionChart()
    {
        ActivityDistributionChart.Clear();

        foreach (var analysis in SelectedAnalyses)
        {
            var dto = analysis.GetActivityDistributionChartData().ToArray();
            var chartGenerator = new StackedBarGenerator(dto, _chartColors);
            if (SelectedAnalyses.Any())
                ActivityDistributionChart.Add(chartGenerator.GenerateChart($"{analysis.FileName}",
                    "Aktivitätsverteilung pro Tag"));
        }
    }

    private void CreateStepsChart()
    {
        StepsCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses) dtos.Add(analysis.GetStepsChartData());

        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        if (SelectedAnalyses.Any())
            StepsCharts.Add(chartGenerator.GenerateChart("Schritte pro Tag", "Durchschnittliche Schritte pro Tag"));
    }
    #endregion
}