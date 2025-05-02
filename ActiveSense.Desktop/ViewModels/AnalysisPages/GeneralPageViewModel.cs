using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class GeneralPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private bool _chartsVisible;
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = [];
    
    [ObservableProperty] private string _sleepStepsTitle = "Schlaf- und Aktivitätsverteilung";
    [ObservableProperty] private string _sleepStepsDescription = "Tägliche Schritte in Relation zur Schlafeffizienz";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _sleepStepsCharts = [];
    [ObservableProperty] private bool _isSleepStepsExpanded = false;

    [ObservableProperty] private string _movementTitle = "Aktivitätsverteilung";
    [ObservableProperty] private string _movementDescription = "Die durchschnittliche Verteilung der Aktivität über 24h";
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _movementPieCharts = [];
    [ObservableProperty] private bool _isMovementExpanded = false;

    public GeneralPageViewModel(SharedDataService sharedDataService, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
    }

    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreateStepsChart();
        CreateMovementPieChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    #region Chart generation

    private void CreateMovementPieChart()
    {
        MovementPieCharts.Clear();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is not (IActivityAnalysis activityAnalysis and IChartDataProvider chartProvider)) continue;
            var dto = chartProvider.GetMovementPatternChartData();
            var pieChartGenerator = new PieChartGenerator(dto, _chartColors);
            if (SelectedAnalyses.Any())
                MovementPieCharts.Add(pieChartGenerator.GenerateChart($"{analysis.FileName}",
                    "Die durchschnittliche Verteilung der Aktivität über 24h"));
        }
    }

    private void CreateStepsChart()
    {
        SleepStepsCharts.Clear();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is not (IActivityAnalysis activityAnalysis and ISleepAnalysis sleepAnalysis)) continue;
            var line = new List<ChartDataDTO>();
            var bar = new List<ChartDataDTO>();

            var labels = activityAnalysis.ActivityWeekdays();

            bar.Add(new ChartDataDTO
            {
                Data = activityAnalysis.StepsPerDay,
                Labels = labels,
                Title = "Schritte pro Tag"
            });
            line.Add(new ChartDataDTO
            {
                Data = sleepAnalysis.SleepEfficiency,
                Labels = labels,
                Title = "Schlaf-Effizienz"
            });
            var chartGenerator = new BarChartGenerator(bar.ToArray(), _chartColors, line.ToArray());
            SleepStepsCharts.Add(chartGenerator.GenerateChart($"{analysis.FileName}", ""));
        }
    }

    #endregion
}