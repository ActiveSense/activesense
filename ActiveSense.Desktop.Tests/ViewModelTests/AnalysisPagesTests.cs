using System;
using System.Collections.ObjectModel;
using System.IO;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.ViewModels.AnalysisPages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.ViewModelTests;

[TestFixture]
public class AnalysisPagesTests
{
    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        _dateConverter = new DateToWeekdayConverter();
        SetupTestAnalyses();

        _mockSharedDataService = new Mock<ISharedDataService>();
        _mockSharedDataService.Setup(s => s.AllAnalyses).Returns(_testAnalyses);
        _mockSharedDataService.Setup(s => s.SelectedAnalyses).Returns(_selectedAnalyses);

        var services = new ServiceCollection();

        var logger = new LoggerConfiguration().CreateLogger();
        services.AddSingleton<ILogger>(logger);

        services.AddSingleton(_mockSharedDataService.Object);

        services.AddSingleton<ChartColors>();
        services.AddSingleton<DateToWeekdayConverter>();

        services.AddSingleton<SleepPageViewModel>();
        services.AddSingleton<ActivityPageViewModel>();
        services.AddSingleton<GeneralPageViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _sleepViewModel = _serviceProvider.GetRequiredService<SleepPageViewModel>();
        _activityViewModel = _serviceProvider.GetRequiredService<ActivityPageViewModel>();
        _generalViewModel = _serviceProvider.GetRequiredService<GeneralPageViewModel>();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
        _serviceProvider.Dispose();
    }

    private SleepPageViewModel _sleepViewModel;
    private ActivityPageViewModel _activityViewModel;
    private GeneralPageViewModel _generalViewModel;
    private Mock<ISharedDataService> _mockSharedDataService;
    private ObservableCollection<IAnalysis> _testAnalyses;
    private ObservableCollection<IAnalysis> _selectedAnalyses;
    private ServiceProvider _serviceProvider;
    private string _tempDir;
    private DateToWeekdayConverter _dateConverter;

    private void SetupTestAnalyses()
    {
        _testAnalyses = new ObservableCollection<IAnalysis>();
        _selectedAnalyses = new ObservableCollection<IAnalysis>();

        // Create test sleep analysis with sleep records
        var sleepAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "SleepAnalysis",
            FilePath = Path.Combine(_tempDir, "sleep")
        };

        sleepAnalysis.SetSleepRecords(new[]
        {
            new SleepRecord
            {
                NightStarting = "2024-11-29",
                SleepOnsetTime = "21:25",
                RiseTime = "06:58",
                TotalElapsedBedTime = "34225",
                TotalSleepTime = "26676",
                TotalWakeTime = "7549",
                SleepEfficiency = "77.9",
                NumActivePeriods = "50",
                MedianActivityLength = "124"
            }
        });

        sleepAnalysis.AddTag("Schlafdaten", "#3277a8");

        // Create test activity analysis with activity records
        var activityAnalysis = new GeneActiveAnalysis(_dateConverter)
        {
            FileName = "ActivityAnalysis",
            FilePath = Path.Combine(_tempDir, "activity")
        };

        activityAnalysis.SetActivityRecords(new[]
        {
            new ActivityRecord
            {
                Day = "2024-10-03",
                Steps = "3624",
                NonWear = "0",
                Sleep = "12994",
                Sedentary = "26283",
                Light = "14007",
                Moderate = "3286",
                Vigorous = "0"
            }
        });

        activityAnalysis.AddTag("Aktivitätsdaten", "#38a832");

        _testAnalyses.Add(sleepAnalysis);
        _testAnalyses.Add(activityAnalysis);

        _selectedAnalyses.Clear();
    }

    [Test]
    public void SleepPageViewModel_Constructor_InitializesProperties()
    {
        // Assert
        Assert.That(_sleepViewModel, Is.Not.Null);
        Assert.That(_sleepViewModel.SelectedAnalyses, Is.Not.Null);
        Assert.That(_sleepViewModel.SelectedAnalyses.Count, Is.EqualTo(0));
        Assert.That(_sleepViewModel.ChartsVisible, Is.False);

        // Verify titles are initialized
        Assert.That(_sleepViewModel.SleepDistributionTitle, Is.EqualTo("Schlafverteilung"));
        Assert.That(_sleepViewModel.SleepTimeWithEfficiencyTitle, Is.EqualTo("Schlafzeit mit Effizienz"));
        Assert.That(_sleepViewModel.SleepTimeTitle, Is.EqualTo("Schlafzeit"));
        Assert.That(_sleepViewModel.TotalSleepTitle, Is.EqualTo("Schlafzeit"));
        Assert.That(_sleepViewModel.SleepEfficiencyTitle, Is.EqualTo("Schlaf-Effizienz"));
        Assert.That(_sleepViewModel.ActivePeriodsTitle, Is.EqualTo("Aktive Perioden"));
    }

    [Test]
    public void SleepPageViewModel_WhenSelectingAnalysis_CreatesPieCharts()
    {
        // Arrange
        var sleepAnalysis = _testAnalyses[0]; // Get the sleep analysis

        // Act
        _selectedAnalyses.Add(sleepAnalysis);
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_sleepViewModel.ChartsVisible, Is.True);
        Assert.That(_sleepViewModel.PieCharts.Count, Is.EqualTo(1));
    }

    [Test]
    public void SleepPageViewModel_WhenNothingSelected_HidesCharts()
    {
        // Arrange - First select something
        var sleepAnalysis = _testAnalyses[0];
        _selectedAnalyses.Add(sleepAnalysis);
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Act - Then clear selection
        _selectedAnalyses.Clear();
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_sleepViewModel.ChartsVisible, Is.False);
    }

    [Test]
    public void ActivityPageViewModel_Constructor_InitializesProperties()
    {
        // Assert
        Assert.That(_activityViewModel, Is.Not.Null);
        Assert.That(_activityViewModel.SelectedAnalyses, Is.Not.Null);
        Assert.That(_activityViewModel.SelectedAnalyses.Count, Is.EqualTo(0));
        Assert.That(_activityViewModel.ChartsVisible, Is.False);

        // Verify titles are initialized
        Assert.That(_activityViewModel.ActivityDistributionTitle, Is.EqualTo("Aktivitätsverteilung"));
        Assert.That(_activityViewModel.StepsTitle, Is.EqualTo("Schritte pro Tag"));
        Assert.That(_activityViewModel.SedentaryTitle, Is.EqualTo("Inaktive Zeit"));
        Assert.That(_activityViewModel.LightTitle, Is.EqualTo("Leichte Aktivität"));
        Assert.That(_activityViewModel.ModerateTitle, Is.EqualTo("Mittlere Aktivität"));
        Assert.That(_activityViewModel.VigorousTitle, Is.EqualTo("Intensive Aktivität"));
    }

    [Test]
    public void ActivityPageViewModel_WhenSelectingAnalysis_CreatesActivityCharts()
    {
        // Arrange
        var activityAnalysis = _testAnalyses[1]; // Get the activity analysis

        // Act
        _selectedAnalyses.Add(activityAnalysis);
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_activityViewModel.ChartsVisible, Is.True);
        Assert.That(_activityViewModel.ActivityDistributionChart.Count, Is.EqualTo(1));
    }

    [Test]
    public void ActivityPageViewModel_WhenNothingSelected_HidesCharts()
    {
        // Arrange - First select something
        var activityAnalysis = _testAnalyses[1];
        _selectedAnalyses.Add(activityAnalysis);
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Act - Then clear selection
        _selectedAnalyses.Clear();
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_activityViewModel.ChartsVisible, Is.False);
    }

    [Test]
    public void GeneralPageViewModel_Constructor_InitializesProperties()
    {
        // Assert
        Assert.That(_generalViewModel, Is.Not.Null);
        Assert.That(_generalViewModel.SelectedAnalyses, Is.Not.Null);
        Assert.That(_generalViewModel.SelectedAnalyses.Count, Is.EqualTo(0));
        Assert.That(_generalViewModel.ChartsVisible, Is.False);

        // Verify titles are initialized
        Assert.That(_generalViewModel.MovementTitle, Is.EqualTo("Aktivitätsverteilung"));
        Assert.That(_generalViewModel.AverageSleepTitle, Is.EqualTo("Durchschnittlicher Schlaf"));
        Assert.That(_generalViewModel.AverageActivityTitle, Is.EqualTo("Durchschnittliche Aktivität"));
    }

    [Test]
    public void GeneralPageViewModel_WhenSelectingBothAnalyses_CreatesAllCharts()
    {
        // Arrange - Select both analyses
        _selectedAnalyses.Add(_testAnalyses[0]); // Sleep analysis
        _selectedAnalyses.Add(_testAnalyses[1]); // Activity analysis

        // Act
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_generalViewModel.ChartsVisible, Is.True);
        Assert.That(_generalViewModel.MovementPieCharts.Count, Is.GreaterThan(0));
        Assert.That(_generalViewModel.AverageSleepCharts.Count, Is.GreaterThan(0));
        Assert.That(_generalViewModel.AverageActivityCharts.Count, Is.GreaterThan(0));
    }

    [Test]
    public void GeneralPageViewModel_WhenNothingSelected_HidesCharts()
    {
        // Arrange - First select something
        _selectedAnalyses.Add(_testAnalyses[0]);
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Act - Then clear selection
        _selectedAnalyses.Clear();
        _mockSharedDataService.Raise(s => s.SelectedAnalysesChanged += null, EventArgs.Empty);

        // Assert
        Assert.That(_generalViewModel.ChartsVisible, Is.False);
    }
}