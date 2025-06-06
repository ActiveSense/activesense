using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly ISharedDataService _sharedDataService;
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = [];

    [ObservableProperty] private bool _chartsVisible;

    [ObservableProperty] private string _sleepDistributionTitle = "Schlafverteilung";
    [ObservableProperty] private string _sleepDistributionDescription = "Durchschnittliche Verteilung der Schlaf- und Wachzeiten in Stunden pro Nacht";
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _pieCharts = [];
    [ObservableProperty] private bool _isDistributionExpanded;

    [ObservableProperty] private string _sleepTimeWithEfficiencyTitle = "Schlafzeit mit Effizienz";
    [ObservableProperty] private string _sleepTimeWithEfficiencyDescription = "Schlafzeit pro Nacht in Stunden mit Schlafeffizienz in %";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _sleepTimeWithEfficiencyCharts = [];
    [ObservableProperty] private bool _isSleepTimeWithEfficiencyExpanded;

    [ObservableProperty] private string _sleepTimeTitle = "Schlafzeit";
    [ObservableProperty] private string _sleepTimeDescription = "Zeitpunkt des Schlafbeginns und -endes";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _sleepTimeCharts = [];
    [ObservableProperty] private bool _isSleepTimeExpanded;

    [ObservableProperty] private string _totalSleepTitle = "Schlafzeit";
    [ObservableProperty] private string _totalSleepDescription = "Durchschnittliche Schlafzeit pro Nacht in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _totalSleepCharts = [];
    [ObservableProperty] private bool _isTotalSleepExpanded;

    [ObservableProperty] private string _sleepEfficiencyTitle = "Schlaf-Effizienz";
    [ObservableProperty] private string _sleepEfficiencyDescription = "Schlaf-Effizienz pro Nacht in %";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _sleepEfficiencyCharts = [];
    [ObservableProperty] private bool _isSleepEfficiencyExpanded;

    [ObservableProperty] private string _activePeriodsTitle = "Aktive Perioden";
    [ObservableProperty] private string _activePeriodsDescription = "Aktive Perioden pro Nacht";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _activePeriodsCharts = [];
    [ObservableProperty] private bool _isActivePeriodsExpanded;


    public SleepPageViewModel(ISharedDataService sharedDataService,
        ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;
        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        UpdateSelectedAnalyses();
    }


    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreatePieCharts();
        CreateTotalSleepChart();
        CreateSleepEfficiencyChart();
        CreateActivePeriodsChart();
        CreateTotalSleepWithEfficiencyChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    #region ChartsGeneration

    private void CreatePieCharts()
    {
        PieCharts.Clear();

        foreach (var analysis in SelectedAnalyses)
            if (analysis is IChartDataProvider chartProvider &&
                analysis is ISleepAnalysis sleepAnalysis)
            {
                var dto = chartProvider.GetSleepDistributionChartData();
                var pieChartGenerator = new PieChartGenerator(dto, _chartColors);
                if (SelectedAnalyses.Any())
                    PieCharts.Add(pieChartGenerator.GenerateChart(
                        $"{analysis.FileName} ({sleepAnalysis.GetSleepDateRange()})",
                        "Durchschnittliche Verteilung der Schlaf- und Wachzeiten in Stunden"));
            }
    }

    private void CreateTotalSleepChart()
    {
        TotalSleepCharts.Clear();

        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses)
            if (analysis is IChartDataProvider chartProvider)
                chartDataDtos.Add(chartProvider.GetTotalSleepTimePerDayChartData());

        var barChartGenerator = new BarChartGenerator(chartDataDtos.ToArray(), _chartColors);
        if (SelectedAnalyses.Any())
            TotalSleepCharts.Add(barChartGenerator.GenerateChart("Total Sleep Time",
                "Durchschnittliche Schlafzeit pro Nacht"));
    }

    private void CreateTotalSleepWithEfficiencyChart()
    {
        SleepTimeWithEfficiencyCharts.Clear();

        foreach (var analysis in SelectedAnalyses)
        {
            var chartDataDtos = new List<ChartDataDTO>();
            var sleepEfficiencyDtos = new List<ChartDataDTO>();

            if (analysis is ISleepAnalysis sleepAnalysis &&
                analysis is IChartDataProvider chartProvider)
            {
                chartDataDtos.Add(chartProvider.GetTotalSleepTimePerDayChartData());
                sleepEfficiencyDtos.Add(chartProvider.GetSleepEfficiencyChartData());
                var barChartGenerator =
                    new BarChartGenerator(chartDataDtos.ToArray(), _chartColors, sleepEfficiencyDtos.ToArray());
                SleepTimeWithEfficiencyCharts.Add(barChartGenerator.GenerateChart(
                    $"{analysis.FileName} ({sleepAnalysis.GetSleepDateRange()})",
                    "Sleep Time per Night with Efficiency"));
            }
        }
    }

    private void CreateSleepEfficiencyChart()
    {
        SleepEfficiencyCharts.Clear();

        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses)
            if (analysis is IChartDataProvider chartProvider)
                chartDataDtos.Add(chartProvider.GetSleepEfficiencyChartData());

        var barChartGenerator = new BarChartGenerator(null, _chartColors, chartDataDtos.ToArray());
        if (SelectedAnalyses.Any())
            SleepEfficiencyCharts.Add(barChartGenerator.GenerateChart("Sleep Efficiency",
                "Durchschnittliche Schlaf-Effizienz pro Nacht"));
    }

    private void CreateActivePeriodsChart()
    {
        ActivePeriodsCharts.Clear();

        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses)
            if (analysis is IChartDataProvider chartProvider)
                chartDataDtos.Add(chartProvider.GetActivePeriodsChartData());

        var barChartGenerator = new BarChartGenerator(chartDataDtos.ToArray(), _chartColors);
        if (SelectedAnalyses.Any())
            ActivePeriodsCharts.Add(barChartGenerator.GenerateChart("Active Periods",
                "Aktive Perioden pro Nacht"));
    }

    #endregion
}