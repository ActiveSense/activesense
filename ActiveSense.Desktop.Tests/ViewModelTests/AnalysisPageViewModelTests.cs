using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Domain.Models;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.AnalysisPages;
using ActiveSense.Desktop.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.ViewModelTests
{
    [TestFixture]
    public class AnalysisPageViewModelTests
    {
        private AnalysisPageViewModel _viewModel;
        private Mock<ISharedDataService> _mockSharedDataService;
        private Mock<IResultParser> _mockResultParser;
        private Mock<IPathService> _mockPathService;
        private ObservableCollection<IAnalysis> _testAnalyses;
        private ObservableCollection<IAnalysis> _selectedAnalyses;
        private ServiceProvider _serviceProvider;
        private string _tempDir;
        private Mock<MainViewModel> _mockMainViewModel;
        private Mock<DialogViewModel> _mockDialogViewModel;
        private DateToWeekdayConverter _dateConverter;

        [SetUp]
        public void Setup()
        {
            // Create a temp directory for any file operations
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);

            // Setup test data
            _dateConverter = new DateToWeekdayConverter();
            SetupTestAnalyses();

            // Setup mocks for interfaces we need to control
            _mockSharedDataService = new Mock<ISharedDataService>();
            _mockSharedDataService.Setup(s => s.AllAnalyses).Returns(_testAnalyses);
            _mockSharedDataService.Setup(s => s.SelectedAnalyses).Returns(_selectedAnalyses);
            
            _mockResultParser = new Mock<IResultParser>();
            _mockResultParser.Setup(r => r.GetAnalysisPages()).Returns(new[] { 
                ApplicationPageNames.Allgemein, 
                ApplicationPageNames.Aktivität, 
                ApplicationPageNames.Schlaf 
            });

            _mockPathService = new Mock<IPathService>();
            _mockPathService.Setup(p => p.OutputDirectory).Returns(_tempDir);

            // Setup dialog service mocks
            _mockDialogViewModel = new Mock<DialogViewModel>();
            
            // Create a mock for PageFactory with its required constructor parameter
            var mockPageFactoryFunc = Mock.Of<Func<ApplicationPageNames, PageViewModel>>();
            var mockPageFactory = new Mock<PageFactory>(mockPageFactoryFunc);
            
            _mockMainViewModel = new Mock<MainViewModel>(
                _mockDialogViewModel.Object,
                mockPageFactory.Object,
                Mock.Of<DialogService>(),
                _mockPathService.Object);

            // Build service provider with real implementations where needed
            var services = new ServiceCollection();

            // Add a logger
            var logger = new LoggerConfiguration().CreateLogger();
            services.AddSingleton<ILogger>(logger);

            // Add services with mocks
            services.AddSingleton(_mockSharedDataService.Object);
            services.AddSingleton(_mockPathService.Object);
            services.AddSingleton(_mockMainViewModel.Object);
            services.AddSingleton(_mockDialogViewModel.Object);

            // Add real implementations for needed components
            services.AddSingleton<ChartColors>();
            services.AddSingleton<DateToWeekdayConverter>();
            services.AddSingleton<DialogService>();

            // Add parser factory with mock result parser
            services.AddSingleton<Func<SensorTypes, IResultParser>>(_ => _ => _mockResultParser.Object);
            services.AddSingleton<ResultParserFactory>();
            
            // Add sensor processor factory with mock sensor processor
            var mockSensorProcessor = new Mock<ISensorProcessor>();
            mockSensorProcessor.Setup(p => p.ProcessingInfo).Returns("Test processing info");
            mockSensorProcessor.Setup(p => p.DefaultArguments).Returns(new List<ScriptArgument>());
            mockSensorProcessor.Setup(p => p.SupportedType).Returns(SensorTypes.GENEActiv);
            services.AddSingleton<Func<SensorTypes, ISensorProcessor>>(_ => _ => mockSensorProcessor.Object);
            services.AddSingleton<SensorProcessorFactory>();

            // Add page factory with real implementations
            services.AddSingleton<GeneralPageViewModel>();
            services.AddSingleton<ActivityPageViewModel>();
            services.AddSingleton<SleepPageViewModel>();
            services.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(sp => name => name switch
            {
                ApplicationPageNames.Allgemein => sp.GetRequiredService<GeneralPageViewModel>(),
                ApplicationPageNames.Aktivität => sp.GetRequiredService<ActivityPageViewModel>(),
                ApplicationPageNames.Schlaf => sp.GetRequiredService<SleepPageViewModel>(),
                _ => throw new InvalidOperationException($"No ViewModel registered for {name}"),
            });
            services.AddSingleton<PageFactory>();

            // Register process dialog view model
            services.AddSingleton<ProcessDialogViewModel>();
            
            // Register export dialog view model and its dependencies
            services.AddSingleton<Func<bool, Task<string>>>(_ => _ => Task.FromResult("test_path.pdf"));
            
            // Add exporter factory with mock exporter
            var mockExporter = new Mock<IExporter>();
            services.AddSingleton<Func<SensorTypes, IExporter>>(_ => _ => mockExporter.Object);
            services.AddSingleton<ExporterFactory>();
            
            services.AddSingleton<ExportDialogViewModel>();

            // Register the view model under test
            services.AddSingleton<AnalysisPageViewModel>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Get the view model from the service provider
            _viewModel = _serviceProvider.GetRequiredService<AnalysisPageViewModel>();
        }

        private void SetupTestAnalyses()
        {
            _testAnalyses = new ObservableCollection<IAnalysis>();
            _selectedAnalyses = new ObservableCollection<IAnalysis>();

            // Create some test analyses
            var analysis1 = new GeneActiveAnalysis(_dateConverter)
            {
                FileName = "TestAnalysis1",
                FilePath = Path.Combine(_tempDir, "analysis1")
            };
            analysis1.AddTag("Schlafdaten", "#3277a8");

            var analysis2 = new GeneActiveAnalysis(_dateConverter)
            {
                FileName = "TestAnalysis2",
                FilePath = Path.Combine(_tempDir, "analysis2")
            };
            analysis2.AddTag("Aktivitätsdaten", "#38a832");

            // Add to collections
            _testAnalyses.Add(analysis1);
            _testAnalyses.Add(analysis2);
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
            Assert.That(_viewModel.ResultFiles, Is.Not.Null);
            Assert.That(_viewModel.ResultFiles.Count, Is.EqualTo(2));
            Assert.That(_viewModel.SensorType, Is.EqualTo(SensorTypes.GENEActiv));
            Assert.That(_viewModel.ShowSpinner, Is.True);
        }

        [Test]
        public async Task Initialize_LoadsTabItems()
        {
            // Act
            await _viewModel.InitializeCommand.ExecuteAsync(null);

            // Assert
            Assert.That(_viewModel.TabItems, Is.Not.Empty);
            Assert.That(_viewModel.TabItems.Count, Is.EqualTo(3));
            Assert.That(_viewModel.TabItems[0].Name, Is.EqualTo("Allgemein"));
            Assert.That(_viewModel.TabItems[1].Name, Is.EqualTo("Aktivität"));
            Assert.That(_viewModel.TabItems[2].Name, Is.EqualTo("Schlaf"));
            Assert.That(_viewModel.ShowSpinner, Is.False);
            
            // Verify SelectedTabItem is set to the first tab
            Assert.That(_viewModel.SelectedTabItem, Is.EqualTo(_viewModel.TabItems[0]));
        }

        [Test]
        public void SelectedAnalysesChanged_UpdatesSharedDataService()
        {
            // Arrange
            var analysis = _testAnalyses.First();
            var newSelection = new ObservableCollection<IAnalysis> { analysis };
            
            // Act
            _viewModel.SelectedAnalyses = newSelection;
            
            // Assert
            _mockSharedDataService.Verify(s => s.UpdateSelectedAnalyses(It.Is<ObservableCollection<IAnalysis>>(
                collection => collection.Count == 1 && collection.First() == analysis)), Times.Once);
        }

        
        [Test]
        public void OnAnalysesChanged_UpdatesResultFiles()
        {
            // Arrange
            var newAnalysis = new GeneActiveAnalysis(_dateConverter)
            {
                FileName = "NewAnalysis",
                FilePath = Path.Combine(_tempDir, "new")
            };
            
            // Act
            _mockSharedDataService.Raise(s => s.AllAnalysesChanged += null, EventArgs.Empty);
            
            // Assert
            Assert.That(_viewModel.ResultFiles, Is.SameAs(_testAnalyses));
        }
        
        [Test]
        public void ShowExportOption_TrueWhenExactlyOneAnalysisSelected()
        {
            // Arrange
            var analysis = _testAnalyses.First();
            
            // Act - Set exactly one analysis
            _viewModel.SelectedAnalyses = new ObservableCollection<IAnalysis> { analysis };
            
            // Assert
            Assert.That(_viewModel.ShowExportOption, Is.True);
            
            // Act - Set no analyses
            _viewModel.SelectedAnalyses = new ObservableCollection<IAnalysis>();
            
            // Assert
            Assert.That(_viewModel.ShowExportOption, Is.False);
            
            // Act - Set multiple analyses
            _viewModel.SelectedAnalyses = new ObservableCollection<IAnalysis> { _testAnalyses[0], _testAnalyses[1] };
            
            // Assert
            Assert.That(_viewModel.ShowExportOption, Is.False);
        }
    }
}