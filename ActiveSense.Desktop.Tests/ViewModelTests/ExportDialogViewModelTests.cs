using System;
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

namespace ActiveSense.Desktop.Tests.ViewModelTests
{
    [TestFixture]
    public class ExportDialogViewModelTests
    {
        private ExportDialogViewModel _viewModel;
        private Mock<ISharedDataService> _mockSharedDataService;
        private Mock<MainViewModel> _mockMainViewModel;
        private Mock<IExporter> _mockExporter;
        private ObservableCollection<IAnalysis> _selectedAnalyses;
        private GeneActiveAnalysis _testAnalysis;
        private ServiceProvider _serviceProvider;
        private string _tempDir;
        private Mock<Func<bool, Task<string>>> _mockFilePickerFunc;
        private bool _filePicked = false;
        private string _pickedFilePath = "test_output.pdf";

        [SetUp]
        public void Setup()
        {
            // Create a temp directory for any file operations
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);

            // Setup test data
            var dateConverter = new DateToWeekdayConverter();
            _testAnalysis = new GeneActiveAnalysis(dateConverter)
            {
                FileName = "TestAnalysis",
                FilePath = Path.Combine(_tempDir, "analysis")
            };

            _selectedAnalyses = new ObservableCollection<IAnalysis> { _testAnalysis };

            // Setup mocks for interfaces we need to control
            _mockSharedDataService = new Mock<ISharedDataService>();
            _mockSharedDataService.Setup(s => s.SelectedAnalyses).Returns(_selectedAnalyses);
            _mockSharedDataService.Setup(s => s.UpdateAllAnalyses(It.IsAny<System.Collections.Generic.IEnumerable<IAnalysis>>()));

            // Setup mock exporter
            _mockExporter = new Mock<IExporter>();
            _mockExporter.Setup(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            // Setup mock file picker function
            _mockFilePickerFunc = new Mock<Func<bool, Task<string>>>();
            _mockFilePickerFunc.Setup(f => f(It.IsAny<bool>()))
                .Callback<bool>(_ => _filePicked = true)
                .ReturnsAsync(() => string.IsNullOrEmpty(_pickedFilePath) ? null : _pickedFilePath);

            // Setup mock main view model
            var mockPageFactoryFunc = Mock.Of<Func<ApplicationPageNames, PageViewModel>>();
            var mockPageFactory = new Mock<PageFactory>(mockPageFactoryFunc);
            
            _mockMainViewModel = new Mock<MainViewModel>(
                Mock.Of<DialogViewModel>(),
                mockPageFactory.Object,
                Mock.Of<DialogService>(),
                Mock.Of<IPathService>());

            // Build service provider with real implementations where needed
            var services = new ServiceCollection();

            // Add logger
            var logger = new LoggerConfiguration().CreateLogger();
            services.AddSingleton<ILogger>(logger);

            // Add mocked services
            services.AddSingleton(_mockSharedDataService.Object);
            services.AddSingleton(_mockMainViewModel.Object);

            // Add mock dialog service
            services.AddSingleton<DialogService>();

            // Add exporter factory with mock exporter
            services.AddSingleton<Func<SensorTypes, IExporter>>(_ => _ => _mockExporter.Object);
            services.AddSingleton<ExporterFactory>();

            // Register the file picker function
            services.AddSingleton(_mockFilePickerFunc.Object);

            // Register the view model under test
            services.AddSingleton<ExportDialogViewModel>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Get the view model from the service provider
            _viewModel = _serviceProvider.GetRequiredService<ExportDialogViewModel>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up temp directory
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }

            // Dispose service provider
            _serviceProvider.Dispose();
        }

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
            _viewModel.FilePickerRequested += (includeRaw) => Task.FromResult(_pickedFilePath);

            // Act
            await _viewModel.ExportAnalysisCommand.ExecuteAsync(null);

            // Assert
            _mockExporter.Verify(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            Assert.That(_testAnalysis.Exported, Is.False);
        }

        [Test]
        public async Task ExportAnalysis_SetsExportStartedFlagDuringExport()
        {
            // Arrange
            bool exportStartedDuringExport = false;
            
            _mockExporter.Setup(e => e.ExportAsync(It.IsAny<IAnalysis>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback(() => exportStartedDuringExport = _viewModel.ExportStarted)
                .ReturnsAsync(true);
                
            _viewModel.FilePickerRequested += (includeRaw) => Task.FromResult(_pickedFilePath);

            // Act
            await _viewModel.ExportAnalysisCommand.ExecuteAsync(null);

            // Assert
            Assert.That(exportStartedDuringExport, Is.True, "ExportStarted should be true during export");
            Assert.That(_viewModel.ExportStarted, Is.False, "ExportStarted should be reset after export");
        }
        
       
    }
}