using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.AnalysisPages;
using ActiveSense.Desktop.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Core;

namespace ActiveSense.Desktop.Tests.ViewModelTests
{
    [TestFixture]
    public class ProcessDialogViewModelTests
    {
        private ProcessDialogViewModel _viewModel;
        private Mock<ISharedDataService> _mockSharedDataService;
        private Mock<IPathService> _mockPathService;
        private Mock<ISensorProcessor> _mockSensorProcessor;
        private string _tempDir;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            // Create a temp directory for any file operations
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);

            // Setup mocks for interfaces we need to control
            _mockSharedDataService = new Mock<ISharedDataService>();
            _mockPathService = new Mock<IPathService>();
            _mockSensorProcessor = new Mock<ISensorProcessor>();

            // Configure the mock sensor processor
            _mockSensorProcessor.Setup(p => p.SupportedType).Returns(SensorTypes.GENEActiv);
            _mockSensorProcessor.Setup(p => p.ProcessingInfo).Returns("Test processing info");
            _mockSensorProcessor.Setup(p => p.DefaultArguments).Returns(new List<ScriptArgument> {
                new BoolArgument { Flag = "test", Name = "Test Argument", Description = "Test Description", Value = true }
            });

            // Build service provider with real implementations where needed
            var services = new ServiceCollection();

            // Add a logger
            var logger = new LoggerConfiguration().CreateLogger();
            services.AddSingleton<ILogger>(logger);

            // Add services with mocks
            services.AddSingleton(_mockSharedDataService.Object);
            services.AddSingleton(_mockPathService.Object);

            // Add a real function that returns our mock processor
            services.AddSingleton<Func<SensorTypes, ISensorProcessor>>(_ => _ => _mockSensorProcessor.Object);
            
            // Add a real function that returns a mock result parser
            var mockResultParser = new Mock<IResultParser>();
            services.AddSingleton<Func<SensorTypes, IResultParser>>(_ => _ => mockResultParser.Object);

            // Add factories with real implementations
            services.AddSingleton<SensorProcessorFactory>();
            services.AddSingleton<ResultParserFactory>();

            // Add other required services
            services.AddSingleton<DialogService>();
            services.AddSingleton<DialogViewModel>();
            services.AddSingleton<DateToWeekdayConverter>();
            services.AddSingleton<ChartColors>();

            // Add page factory dependencies
            services.AddSingleton<AnalysisPageViewModel>();
            services.AddSingleton<SleepPageViewModel>();
            services.AddSingleton<ActivityPageViewModel>();
            services.AddSingleton<GeneralPageViewModel>();

            // Add the page factory
            services.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(sp => name => name switch
            {
                ApplicationPageNames.Analyse => sp.GetRequiredService<AnalysisPageViewModel>(),
                ApplicationPageNames.Schlaf => sp.GetRequiredService<SleepPageViewModel>(),
                ApplicationPageNames.Aktivität => sp.GetRequiredService<ActivityPageViewModel>(),
                ApplicationPageNames.Allgemein => sp.GetRequiredService<GeneralPageViewModel>(),
                _ => throw new InvalidOperationException($"No ViewModel registered for {name}"),
            });
            services.AddSingleton<PageFactory>();

            // Add main view model
            services.AddSingleton<MainViewModel>();

            // Add the ProcessDialogViewModel
            services.AddSingleton<ProcessDialogViewModel>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Get the view model from the service provider
            _viewModel = _serviceProvider.GetRequiredService<ProcessDialogViewModel>();
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
            Assert.That(_viewModel.Title, Is.EqualTo("Sensordaten analysieren"));
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("No files selected"));
            Assert.That(_viewModel.SelectedSensorTypes, Is.EqualTo(SensorTypes.GENEActiv));
            Assert.That(_viewModel.ProcessingInfo, Is.EqualTo("Test processing info"));
            Assert.That(_viewModel.Arguments, Is.Not.Empty);
        }

        [Test]
        public void SetSelectedFiles_WithNullFiles_UpdatesStatusMessage()
        {
            // Act
            _viewModel.SetSelectedFiles(null);

            // Assert
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("No files selected"));
            Assert.That(_viewModel.SelectedFiles, Is.Null);
        }

        [Test]
        public void SetSelectedFiles_WithEmptyFiles_UpdatesStatusMessage()
        {
            // Act
            _viewModel.SetSelectedFiles(Array.Empty<string>());

            // Assert
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("No files selected"));
            Assert.That(_viewModel.SelectedFiles, Is.Empty);
        }

        [Test]
        public void SetSelectedFiles_WithFiles_UpdatesStatusMessageWithCount()
        {
            // Arrange
            var files = new[] { "file1.bin", "file2.bin" };

            // Act
            _viewModel.SetSelectedFiles(files);

            // Assert
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("2 file(s) selected"));
            Assert.That(_viewModel.SelectedFiles, Is.SameAs(files));
        }

        [Test]
        public void CancelCommand_WhenNotProcessing_CanExecute()
        {
            // Arrange
            _viewModel.IsProcessing = false;

            // Act
            var canExecute = _viewModel.CancelCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void HideCommand_CanExecute()
        {
            // Act
            var canExecute = _viewModel.HideCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public async Task ProcessFilesCommand_WithNoFiles_UpdatesStatusMessageOnly()
        {
            // Arrange
            _viewModel.SetSelectedFiles(null);

            // Act
            await _viewModel.ProcessFilesCommand.ExecuteAsync(null);

            // Assert
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("No files selected"));
            _mockSensorProcessor.Verify(p => p.ProcessAsync(It.IsAny<IEnumerable<ScriptArgument>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Arguments_AreLoadedFromSensorProcessor()
        {
            // Assert
            Assert.That(_viewModel.Arguments.Count, Is.EqualTo(1));
            var arg = _viewModel.Arguments[0] as BoolArgument;
            Assert.That(arg, Is.Not.Null);
            Assert.That(arg.Flag, Is.EqualTo("test"));
            Assert.That(arg.Name, Is.EqualTo("Test Argument"));
            Assert.That(arg.Description, Is.EqualTo("Test Description"));
            Assert.That(arg.Value, Is.True);
        }
    }
}