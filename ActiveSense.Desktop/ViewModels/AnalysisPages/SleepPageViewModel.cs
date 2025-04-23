using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    
    [ObservableProperty] private bool _chartsVisible = false;
    
    [ObservableProperty] private string _sleepDistributionTitle = "Schlafverteilung";
    [ObservableProperty] private string _sleepDistributionDescription = "Verteilung der Schlaf- und Wachzeiten";
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _pieCharts = new();
    
    [ObservableProperty] private string _totalSleepTitle = "Schlafzeit";
    [ObservableProperty] private string _totalSleepDescription = "Durchschnittliche Schlafzeit pro Nacht";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _totalSleepCharts = new();

    public SleepPageViewModel(SharedDataService sharedDataService,
        ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        UpdateSelectedAnalyses();
        InitializeCharts();
    }


    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreatePieCharts();
        CreateTotalSleepChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    public void InitializeCharts()
    {
        CreatePieCharts();
        CreateTotalSleepChart();
    }

    #region ChartsGeneration

    private void CreateTotalSleepChart()
    {
        TotalSleepCharts.Clear();
        
        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses) chartDataDtos.Add(analysis.GetTotalSleepTimePerDayChartData());

        var barChartGenerator = new BarChartGenerator(chartDataDtos.ToArray(), _chartColors);
        if (SelectedAnalyses.Any())
            TotalSleepCharts.Add(barChartGenerator.GenerateChart("Total Sleep Time",
                "Durchschnittliche Schlafzeit pro Nacht"));
    }

    public void CreatePieCharts()
    {
        PieCharts.Clear();

        foreach (var analysis in SelectedAnalyses)
        {
            var dto = analysis.GetSleepChartData();
            var pieChartGenerator = new PieChartGenerator(dto, _chartColors);
            if (SelectedAnalyses.Any())
                PieCharts.Add(pieChartGenerator.GenerateChart($"Schlafverteilung {analysis.FileName}",
                    "Verteilung der Schlaf- und Wachzeiten"));
        }
    }

    #endregion
}