using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels.Charts;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class SleepPageViewModel : PageViewModel
{
    private readonly SharedDataService _sharedDataService;
    private readonly BarChartGenerator _barChartGenerator;
    private readonly PieChartGenerator _pieChartGenerator;

    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private ObservableCollection<AnalysisPieChartViewModel> _pieCharts = new();
    [ObservableProperty] private ISeries[] _totalSleepSeries;
    [ObservableProperty] private ICartesianAxis[] _xAxes;
    [ObservableProperty] private ICartesianAxis[] _yAxes;

    [ObservableProperty] private ISeries[] _pieSeries;

    public SleepPageViewModel(SharedDataService sharedDataService, ChartFactory chartFactory, BarChartGenerator barChartGenerator, PieChartGenerator pieChartGenerator)
    {
        _sharedDataService = sharedDataService;
        _barChartGenerator = barChartGenerator;
        _pieChartGenerator = pieChartGenerator;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;

        UpdateSelectedAnalyses();

        // ChartViewModels = new ObservableCollection<ChartViewModel>
        // {
        //     chartFactory.GetChartViewModel(ChartTypes.SleepEfficiency)
        // };
    }

    // public ObservableCollection<ChartViewModel> ChartViewModels { get; }


    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        (TotalSleepSeries, XAxes, YAxes) = _barChartGenerator.GetSleepTimeChart(SelectedAnalyses);
        
        // Generate pie charts for each analysis
        PieCharts.Clear();
        foreach (var analysis in SelectedAnalyses)
        {
            var pieSeries = _pieChartGenerator.GetPieChartForAnalysis(analysis);
            PieCharts.Add(new AnalysisPieChartViewModel
            {
                Title = analysis.FileName,
                PieSeries = pieSeries
            });
        }
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
    }
}
public class AnalysisPieChartViewModel : ViewModelBase
{
    public string Title { get; set; }
    public ISeries[] PieSeries { get; set; }
}