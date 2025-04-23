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

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class GeneralPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private bool _chartsVisible = false;
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _generalCharts = new();
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _generalCharts2 = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();

    public GeneralPageViewModel(SharedDataService sharedDataService, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        // Subscribe to changes in the shared data
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
    }

    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreateStepsChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    private void CreateStepsChart()
    {
        GeneralCharts.Clear();
        foreach (var analysis in SelectedAnalyses)
        {
            var line = new List<ChartDataDTO>();
            var bar = new List<ChartDataDTO>();
            
            var labels = analysis.ActivityWeekdays();

            bar.Add(new ChartDataDTO
            {
                Data = analysis.StepsPercentage,
                Labels = labels,
                Title = "Schritte pro Tag"
            });
            line.Add(new ChartDataDTO
            {
                Data = analysis.SleepEfficiency,
                Labels = labels,
                Title = "Schlaf-Effizienz"
            });
            var chartGenerator = new BarChartGenerator(bar.ToArray(), _chartColors, line.ToArray());
            GeneralCharts.Add(chartGenerator.GenerateChart("Schritte pro Tag", "Durchschnittliche Schritte pro Tag"));
        }
    }
}