using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _generalCharts = new();
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _generalCharts2 = new();

    [ObservableProperty]
    private string _movementDescription = "Die durchschnittliche Verteilung der Aktivität über 24h";

    [ObservableProperty] private ObservableCollection<PieChartViewModel> _movementPieCharts = new();

    [ObservableProperty] private string _movementTitle = "Aktivitätsverteilung";
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = new();

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
            if (analysis is IActivityAnalysis activityAnalysis &&
                analysis is IChartDataProvider chartProvider)
            {
                var dto = chartProvider.GetMovementPatternChartData();
                var pieChartGenerator = new PieChartGenerator(dto, _chartColors);
                if (SelectedAnalyses.Any())
                    MovementPieCharts.Add(pieChartGenerator.GenerateChart($"{analysis.FileName}",
                        "Die durchschnittliche Verteilung der Aktivität über 24h"));
            }
        }
    }

    private void CreateStepsChart()
    {
        GeneralCharts.Clear();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is  IActivityAnalysis activityAnalysis &&
                analysis is ISleepAnalysis sleepAnalysis &&
                analysis is  IChartDataProvider chartProvider)
            {
                var line = new List<ChartDataDTO>();
                var bar = new List<ChartDataDTO>();

                var labels = activityAnalysis.ActivityWeekdays();

                bar.Add(new ChartDataDTO
                {
                    Data = activityAnalysis.StepsPercentage,
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
                GeneralCharts.Add(chartGenerator.GenerateChart($"{analysis.FileName}", ""));
            }
        }
    }

    #endregion
}