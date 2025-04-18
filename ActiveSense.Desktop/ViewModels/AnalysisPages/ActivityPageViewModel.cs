using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _activityDistribtionChart = new();

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
        ActivityDistribtionChart.Clear();
        
        foreach (var analysis in SelectedAnalyses)
        {
            var dto = new ChartDataDTO[]
            {
                new ChartDataDTO()
                {
                    Data = analysis.LightActivity,
                    Labels = analysis.ActivityWeekdays(),
                    Title = "Leichte Aktivität",
                },
                new ChartDataDTO()
                {
                    Data = analysis.ModerateActivity,
                    Labels = analysis.ActivityWeekdays(),
                    Title = "Mittlere Aktivität",
                },
                new ChartDataDTO()
                {
                    Data = analysis.VigorousActivity,
                    Labels = analysis.ActivityWeekdays(),
                    Title = "Intensive Aktivität",
                }
            };
            var chartGenerator = new StackedBarGenerator(dto, _chartColors);
            ActivityDistribtionChart.Add(chartGenerator.GenerateChart($"Aktivitätsverteilung {analysis.FileName}", "Aktivitätsverteilung pro Tag"));
        }
    }

    private void CreateStepsChart()
    {
        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            var data = analysis.StepsPerDay;
            var labels = analysis.ActivityWeekdays();

            dtos.Add(new ChartDataDTO()
            {
                Data = data,
                Labels = labels,
            });
        }


        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        StepsChart = chartGenerator.GenerateChart("Schritte pro Tag", "Durchschnittliche Schritte pro Tag");
    }
}