using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class ActivityPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    private readonly ChartColors _chartColors;

    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private BarChartViewModel _stepsChart = new();
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _activityDistributionChart = new();

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
        foreach (var analysis in _sharedDataService.SelectedAnalyses)
        {
            SelectedAnalyses.Add(analysis);
        }
    }

    private void CreateActivityDistributionChart()
    {
        ActivityDistributionChart.Clear();
        
        foreach (var analysis in SelectedAnalyses)
        {
            var dto = analysis.GetActivityDistributionChartData().ToArray();
            var chartGenerator = new StackedBarGenerator(dto, _chartColors);
            ActivityDistributionChart.Add(chartGenerator.GenerateChart($"Aktivitätsverteilung {analysis.FileName}", "Aktivitätsverteilung pro Tag"));
        }
    }

    private void CreateStepsChart()
    {
        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            dtos.Add(analysis.GetStepsChartData());
        }

        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        StepsChart = chartGenerator.GenerateChart("Schritte pro Tag", "Durchschnittliche Schritte pro Tag");
    }
}