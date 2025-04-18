using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Charts.ViewModels;
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

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    private readonly DateToWeekdayConverter _dateToWeekdayConverter;

    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private ObservableCollection<PieChartViewModel> _pieCharts = new();
    [ObservableProperty] private BarChartViewModel _totalSleepChart = new();
    [ObservableProperty] private ISeries[] _totalSleepSeries;
    [ObservableProperty] private ICartesianAxis[] _xAxes;
    [ObservableProperty] private ICartesianAxis[] _yAxes;

    [ObservableProperty] private ISeries[] _pieSeries;

    public SleepPageViewModel(SharedDataService sharedDataService, DateToWeekdayConverter dateToWeekdayConverter)
    {
        _sharedDataService = sharedDataService;
        _dateToWeekdayConverter = dateToWeekdayConverter;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;

        UpdateSelectedAnalyses();
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
        // Create chart data DTOs from selected analyses
        var chartDataDtos = CreateSleepChartDataDtos();
        
        // Initialize the bar chart generator with DTOs and axes
        var barChartGenerator = new BarChartGenerator(chartDataDtos);

        // Generate the chart view model
        TotalSleepChart = barChartGenerator.GenerateChart();
        TotalSleepChart.Title = "Total Sleep Time";
        TotalSleepChart.Description = "Sleep duration across nights";
    }

    private ChartDataDTO[] CreateSleepChartDataDtos()
    {
        var chartDataDtos = new List<ChartDataDTO>();

        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis.SleepRecords.Count == 0)
                continue;

            var labels = analysis.SleepRecords
                .Select(r => _dateToWeekdayConverter.ConvertDateToWeekday(r.NightStarting))
                .ToArray();

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

        return chartDataDtos.ToArray();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
    }
}