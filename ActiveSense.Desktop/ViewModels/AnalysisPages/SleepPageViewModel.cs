using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    private readonly DateToWeekdayConverter _dateToWeekdayConverter;
    private readonly ChartColors _chartColors;

    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _pieCharts = new();
    [ObservableProperty] private BarChartViewModel _totalSleepChart = new();
    [ObservableProperty] private RowChartViewModel _sleepTimesChart = new();

    [ObservableProperty] private ISeries[] _pieSeries;

    public SleepPageViewModel(SharedDataService sharedDataService, DateToWeekdayConverter dateToWeekdayConverter, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _dateToWeekdayConverter = dateToWeekdayConverter;
        _chartColors = chartColors;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;
        UpdateSelectedAnalyses();
        InitializeCharts();
    }


    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();

        // Only update charts if there are selected analyses
        if (SelectedAnalyses.Count > 0)
        {
            // Initialize chart data
            InitializeCharts();
        }
        else
        {
            // Reset charts when no analyses are selected
            TotalSleepChart = new BarChartViewModel { Title = "Total Sleep Time" };
            PieCharts.Clear();
        }
    }

    public void InitializeCharts()
    {
        CreatePieCharts();
        CreateTotalSleepChart();
    }

    private void CreateTotalSleepChart()
    {
        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses)
        {
            var labels = analysis.SleepWeekdays();

            var sleepTimes = analysis.SleepRecords
                .Select(r =>
                {
                    double.TryParse(r.TotalSleepTime, out var time);
                    return time / 3600; // Convert to hours
                })
                .ToArray();

            chartDataDtos.Add(new ChartDataDTO
            {
                Labels = labels,
                Data = sleepTimes,
                Title = analysis.FileName
            });
        }

        var barChartGenerator = new BarChartGenerator(chartDataDtos.ToArray(), _chartColors);
        TotalSleepChart = barChartGenerator.GenerateChart("Total Sleep Time", "Durchschnittliche Schlafzeit pro Nacht");
    }

    public void CreatePieCharts()
    {
        PieCharts.Clear();

        foreach (var analysis in SelectedAnalyses)
        {
            var dto = new ChartDataDTO();
            dto.Labels = new[] { "Total Sleep Time", "Total Wake Time" };
            dto.Data = new[] { analysis.TotalSleepTime, analysis.TotalWakeTime };
            var pieChartGenerator = new PieChartGenerator(dto, _chartColors);
            PieCharts.Add(pieChartGenerator.GenerateChart($"Schlafverteilung {analysis.FileName}", "Verteilung der Schlaf- und Wachzeiten"));
        }
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
    }
}