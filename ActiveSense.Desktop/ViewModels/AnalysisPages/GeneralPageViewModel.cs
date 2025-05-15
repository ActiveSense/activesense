using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot.Palettes;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class GeneralPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly ISharedDataService _sharedDataService;
    [ObservableProperty] private bool _chartsVisible;
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = [];

    [ObservableProperty] private string _movementTitle = "Aktivitätsverteilung";
    [ObservableProperty] private string _movementDescription = "Die durchschnittliche Verteilung der Aktivität über 24h in Stunden.";
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _movementPieCharts = [];
    [ObservableProperty] private bool _isMovementExpanded = false;
    
    [ObservableProperty] private string _averageSleepTitle = "Durchschnittlicher Schlaf";
    [ObservableProperty] private string _averageSleepDescription = "Durchschnittlicher Schlaf pro Nacht in Stunden.";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _averageSleepCharts = [];
    [ObservableProperty] private bool _isAverageSleepExpanded = false;
    
    [ObservableProperty] private string _averageActivityTitle = "Durchschnittliche Aktivität";
    [ObservableProperty] private string _averageActivityDescription = "Durchschnittliche Aktivität pro Tag in Stunden. Light, moderate und vigorous activity addiert.";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _averageActivityCharts = [];
    [ObservableProperty] private bool _isAverageActivityExpanded = false;
    

    public GeneralPageViewModel(ISharedDataService sharedDataService, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
    }

    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreateMovementPieChart();
        CreateAverageSleepChart();
        CreateAverageActivityChart();
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
                MovementPieCharts.Add(pieChartGenerator.GenerateChart($"{analysis.FileName} ({activityAnalysis.GetActivityDateRange()})",
                    "Die durchschnittliche Verteilung der Aktivität über 24h"));
        }
    }


    private void CreateAverageSleepChart()
    {
        AverageSleepCharts.Clear();

        var labels = new List<string>();
        var data = new List<double>();

        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is not ISleepAnalysis sleepAnalysis) continue;
        
            double averageSleepTimeInHours = Math.Round(sleepAnalysis.AverageSleepTime / 3600, 2);
            data.Add(averageSleepTimeInHours);
            labels.Add(analysis.FileName);
        }

        var chartData = new ChartDataDTO
        {
            Data = data.ToArray(),
            Labels = labels.ToArray(),
            Title = "Durchschnittlicher Schlaf"
        };

        var chartGenerator = new BarChartGenerator(new[] { chartData }, _chartColors);
        AverageSleepCharts.Add(chartGenerator.GenerateChart("Durchschnittlicher Schlaf pro Analyse", 
            "Vergleich der durchschnittlichen Schlafzeit in Stunden"));
    }
    
    private void CreateAverageActivityChart()
    {
        AverageActivityCharts.Clear();

        var labels = new List<string>();
        var data = new List<double>();

        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is not IActivityAnalysis activityAnalysis) continue;
        
            double averageActivityTimeInHours = Math.Round((activityAnalysis.AverageLightActivity + activityAnalysis.AverageModerateActivity + activityAnalysis.AverageVigorousActivity) / 3600, 2) ;
            data.Add(averageActivityTimeInHours);
            labels.Add(analysis.FileName);
        }

        var chartData = new ChartDataDTO
        {
            Data = data.ToArray(),
            Labels = labels.ToArray(),
            Title = "Durchschnittliche Aktivität"
        };

        var chartGenerator = new BarChartGenerator(new[] { chartData }, _chartColors);
        AverageActivityCharts.Add(chartGenerator.GenerateChart("Durchschnittliche Aktivität pro Analyse", 
            "Vergleich der durchschnittlichen Aktivitätszeit in Stunden"));
    }
    
    

    #endregion
}