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
using CommunityToolkit.Mvvm.ComponentModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;

namespace ActiveSense.Desktop.ViewModels.AnalysisPages;

public partial class ActivityPageViewModel : PageViewModel
{
    private readonly ChartColors _chartColors;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private bool _chartsVisible = false;
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = [];

    [ObservableProperty] private string _activityDistributionTitle = "Aktivitätsverteilung";
    [ObservableProperty] private string _activityDistributionDescription = "Aktivitätsverteilung pro Tag in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _activityDistributionChart = [];
    [ObservableProperty] private bool _isActivityDistributionExpanded = false;

    [ObservableProperty] private string _stepsTitle = "Schritte pro Tag";
    [ObservableProperty] private string _stepsDescription = "Durchschnittliche Schritte pro Tag";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _stepsCharts = [];
    [ObservableProperty] private bool _isStepsExpanded = false;

    [ObservableProperty] private string _sedentaryTitle = "Inaktive Zeit";
    [ObservableProperty] private string _sedentaryDescription = "Inaktive Zeit pro Tag in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _sedentaryCharts = [];
    [ObservableProperty] private bool _isSedentaryExpanded = false;

    [ObservableProperty] private string _lightTitle = "Leichte Aktivität";
    [ObservableProperty] private string _lightDescription = "Leichte Aktivität pro Tag in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _lightCharts = [];
    [ObservableProperty] private bool _isLightExpanded = false;

    [ObservableProperty] private string _moderateTitle = "Mittlere Aktivität";
    [ObservableProperty] private string _moderateDescription = "Mittlere Aktivität pro Tag in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _moderateCharts = [];
    [ObservableProperty] private bool _isModerateExpanded = false;

    [ObservableProperty] private string _vigorousTitle = "Intensive Aktivität";
    [ObservableProperty] private string _vigorousDescription = "Intensive Aktivität pro Tag in Stunden";
    [ObservableProperty] private ObservableCollection<BarChartViewModel> _vigorousCharts = [];
    [ObservableProperty] private bool _isVigorousExpanded = false;

    public ActivityPageViewModel(SharedDataService sharedDataService, ChartColors chartColors)
    {
        _sharedDataService = sharedDataService;
        _chartColors = chartColors;

        _sharedDataService.SelectedAnalysesChanged += OnSelectedAnalysesChanged;

        UpdateSelectedAnalyses();
    }

    private void OnSelectedAnalysesChanged(object? sender, EventArgs e)
    {
        UpdateSelectedAnalyses();
        CreateStepsChart();
        CreateSedentaryChart();
        CreateLightChart();
        CreateModerateChart();
        CreateVigorousChart();
        CreateActivityDistributionChart();
    }

    private void UpdateSelectedAnalyses()
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in _sharedDataService.SelectedAnalyses) SelectedAnalyses.Add(analysis);
        ChartsVisible = SelectedAnalyses.Any();
    }

    #region Chart Generation
    private void CreateActivityDistributionChart()
    {
        ActivityDistributionChart.Clear();

        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IActivityAnalysis activityAnalysis &&
                analysis is IChartDataProvider chartProvider)
            {
                var dto = chartProvider.GetActivityDistributionChartData().ToArray();
                var chartGenerator = new StackedBarGenerator(dto, _chartColors);
                ActivityDistributionChart.Add(chartGenerator.GenerateChart(
                    $"{analysis.FileName}",
                    "Aktivitätsverteilung pro Tag"));
            }
        }
    }

    private void CreateStepsChart()
    {
        StepsCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IChartDataProvider chartProvider)
            {
                dtos.Add(chartProvider.GetStepsChartData());
            }
        }

        if (dtos.Count == 0) return;
        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        StepsCharts.Add(chartGenerator.GenerateChart(
            "Schritte pro Tag",
            "Durchschnittliche Schritte pro Tag"));
    }

    private void CreateSedentaryChart()
    {
        SedentaryCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IChartDataProvider chartProvider)
            {
                dtos.Add(chartProvider.GetSedentaryChartData());
            }
        }

        if (dtos.Count == 0) return;
        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        SedentaryCharts.Add(chartGenerator.GenerateChart(
            "Inaktive Zeit pro Tag",
            "Inaktive Zeit pro Tag"));
    }

    private void CreateLightChart()
    {
        LightCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IChartDataProvider chartProvider)
            {
                dtos.Add(chartProvider.GetLightActivityChartData());
            }
        }

        if (dtos.Count == 0) return;
        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        LightCharts.Add(chartGenerator.GenerateChart(
            "Leichte Aktivität pro Tag",
            "Leichte Aktivität pro Tag"));
    }

    private void CreateModerateChart()
    {
        ModerateCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IChartDataProvider chartProvider)
            {
                dtos.Add(chartProvider.GetModerateActivityChartData());
            }
        }

        if (dtos.Count == 0) return;
        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        ModerateCharts.Add(chartGenerator.GenerateChart(
            "Mittlere Aktivität pro Tag",
            "Mittlere Aktivität pro Tag"));
    }

    private void CreateVigorousChart()
    {
        VigorousCharts.Clear();

        var dtos = new List<ChartDataDTO>();
        foreach (var analysis in SelectedAnalyses)
        {
            if (analysis is IChartDataProvider chartProvider)
            {
                dtos.Add(chartProvider.GetVigorousActivityChartData());
            }
        }

        if (dtos.Count == 0) return;
        var chartGenerator = new BarChartGenerator(dtos.ToArray(), _chartColors);
        VigorousCharts.Add(chartGenerator.GenerateChart(
            "Intensive Aktivität pro Tag",
            "Intensive Aktivität pro Tag"));
    }
    #endregion
}