using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.ViewModelTests;

[TestFixture]
public class ExportDialogViewModelTests
{
    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        var dateConverter = new DateToWeekdayConverter();
        _testAnalysis = new GeneActiveAnalysis(dateConverter)
        {
            FileName = "TestAnalysis",
            FilePath = Path.Combine(_tempDir, "analysis")
        };

        _selectedAnalyses = new ObservableCollection<IAnalysis> { _testAnalysis };

        _mockSharedDataService = new Mock<ISharedDataService>();
        _mockSharedDataService.Setup(s => s.SelectedAnalyses).Returns(_selectedAnalyses);
        _mockSharedDataService.Setup(s => s.UpdateAllAnalyses(It.IsAny<IEnumerable<IAnalysis>>()));

        _mockExporter = new Mock<IExporter>();
        _mockExporter.Setup(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(true);

        _mockFilePickerFunc = new Mock<Func<bool, Task<string>>>();
        _mockFilePickerFunc.Setup(f => f(It.IsAny<bool>()))
            .Callback<bool>(_ => _filePicked = true)
            .ReturnsAsync(() => string.IsNullOrEmpty(_pickedFilePath) ? null : _pickedFilePath);

        var mockPageFactoryFunc = Mock.Of<Func<ApplicationPageNames, PageViewModel>>();
        var mockPageFactory = new Mock<PageFactory>(mockPageFactoryFunc);

        _mockMainViewModel = new Mock<MainViewModel>(
            Mock.Of<DialogViewModel>(),
            mockPageFactory.Object,
            Mock.Of<DialogService>(),
            Mock.Of<IPathService>(),
            _mockSharedDataService.Object);

        var services = new ServiceCollection();

        var logger = new LoggerConfiguration().CreateLogger();
        services.AddSingleton<ILogger>(logger);

        services.AddSingleton(_mockSharedDataService.Object);
        services.AddSingleton(_mockMainViewModel.Object);

        services.AddSingleton<DialogService>();

        services.AddSingleton<Func<SensorTypes, IExporter>>(_ => _ => _mockExporter.Object);
        services.AddSingleton<ExporterFactory>();

        services.AddSingleton(_mockFilePickerFunc.Object);

        services.AddSingleton<ExportDialogViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _viewModel = _serviceProvider.GetRequiredService<ExportDialogViewModel>();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);

        _serviceProvider.Dispose();
    }

    private ExportDialogViewModel _viewModel;
    private Mock<ISharedDataService> _mockSharedDataService;
    private Mock<MainViewModel> _mockMainViewModel;
    private Mock<IExporter> _mockExporter;
    private ObservableCollection<IAnalysis> _selectedAnalyses;
    private GeneActiveAnalysis _testAnalysis;
    private ServiceProvider _serviceProvider;
    private string _tempDir;
    private Mock<Func<bool, Task<string>>> _mockFilePickerFunc;
    private bool _filePicked;
    private string _pickedFilePath = "test_output.pdf";

    [Test]
    public void Constructor_InitializesProperties()
    {
        // Assert
        Assert.That(_viewModel, Is.Not.Null);
        Assert.That(_viewModel.SelectedSensorType, Is.EqualTo(SensorTypes.GENEActiv));
        Assert.That(_viewModel.IncludeRawData, Is.False);
        Assert.That(_viewModel.ExportStarted, Is.False);
        Assert.That(_viewModel.SelectedAnalysesCount, Is.EqualTo(1));
    }

    [Test]
    public void GetFirstSelectedAnalysis_ReturnsCorrectAnalysis()
    {
        // Act
        var result = _viewModel.GetFirstSelectedAnalysis();

        // Assert
        Assert.That(result, Is.SameAs(_testAnalysis));
    }

    [Test]
    public void CancelCommand_CanExecute()
    {
        // Act
        var canExecute = _viewModel.CancelCommand.CanExecute(null);

        // Assert
        Assert.That(canExecute, Is.True);
    }

    [Test]
    public async Task ExportAnalysis_WithCanceledFilePicker_DoesNotExport()
    {
        // Arrange
        _pickedFilePath = null; // Simulate canceled file picker
        _viewModel.FilePickerRequested += includeRaw => Task.FromResult(_pickedFilePath);

        // Act
        await _viewModel.ExportAnalysisCommand.ExecuteAsync(null);

        // Assert
        _mockExporter.Verify(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
        Assert.That(_testAnalysis.Exported, Is.False);
    }

    [Test]
    public async Task ExportAnalysis_SetsExportStartedFlagDuringExport()
    {
        // Arrange
        var exportStartedDuringExport = false;

        _mockExporter.Setup(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Callback(() => exportStartedDuringExport = _viewModel.ExportStarted)
            .ReturnsAsync(true);

        _viewModel.FilePickerRequested += includeRaw => Task.FromResult(_pickedFilePath);

        // Act
        await _viewModel.ExportAnalysisCommand.ExecuteAsync(null);

        // Assert
        Assert.That(exportStartedDuringExport, Is.True, "ExportStarted should be true during export");
        Assert.That(_viewModel.ExportStarted, Is.False, "ExportStarted should be reset after export");
    }
}