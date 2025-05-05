using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Process.Implementations;
using ActiveSense.Desktop.Process.Interfaces;
using ActiveSense.Desktop.Services;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessTests;

[TestFixture]
public class GeneActiveProcessorTests
{
    [SetUp]
    public void Setup()
    {
        _mockScriptService = new Mock<IScriptService>();
        _mockScriptExecutor = new Mock<IScriptExecutor>();
        _mockFileManager = new Mock<IFileManager>();
        _mockTimeEstimator = new Mock<IProcessingTimeEstimator>();

        _processor = new GeneActiveProcessor(
            _mockScriptService.Object,
            _mockScriptExecutor.Object,
            _mockFileManager.Object,
            _mockTimeEstimator.Object);

        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        // Set up the script service mock
        _mockScriptService.Setup(x => x.GetScriptPath()).Returns("/path/to/script.R");
        _mockScriptService.Setup(x => x.GetExecutablePath()).Returns("Rscript");
        _mockScriptService.Setup(x => x.GetScriptBasePath()).Returns("/path/to");
        _mockScriptService.Setup(x => x.GetScriptInputPath()).Returns("/path/to/data");
        _mockScriptService.Setup(x => x.GetScriptOutputPath()).Returns("/path/to/outputs");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    private Mock<IScriptService> _mockScriptService;
    private Mock<IScriptExecutor> _mockScriptExecutor;
    private Mock<IFileManager> _mockFileManager;
    private Mock<IProcessingTimeEstimator> _mockTimeEstimator;
    private GeneActiveProcessor _processor;
    private string _tempDir;

    [Test]
    public void SupportedType_ReturnsGeneActiv()
    {
        Assert.That(_processor.SupportedType, Is.EqualTo(SensorTypes.GENEActiv));
    }

    [Test]
    public void DefaultArguments_ContainsExpectedArguments()
    {
        // Act
        var args = _processor.DefaultArguments;

        // Assert
        Assert.That(args, Is.Not.Null);
        Assert.That(args.Count, Is.EqualTo(2));

        // Check for activity analysis argument
        var activityArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "a");
        Assert.That(activityArg, Is.Not.Null);
        Assert.That((activityArg as BoolArgument)?.Value, Is.True);

        // Check for sleep analysis argument
        var sleepArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "s");
        Assert.That(sleepArg, Is.Not.Null);
        Assert.That((sleepArg as BoolArgument)?.Value, Is.True);
    }

    [Test]
    public void CopyFiles_DelegatesCallToFileManager()
    {
        // Arrange
        string[] files = { "file1.bin", "file2.bin" };
        var processingDir = "/path/to/processing";
        var outputDir = "/path/to/output";

        // Act
        _processor.CopyFiles(files, processingDir, outputDir);

        // Assert
        _mockFileManager.Verify(x => x.CopyFiles(
                files,
                processingDir,
                outputDir,
                It.Is<string[]>(types => types.Contains(".bin"))),
            Times.Once);
    }

    [Test]
    public void GetEstimatedProcessingTime_DelegatesCallToTimeEstimator()
    {
        // Arrange
        string[] files = { "file1.bin", "file2.bin" };
        var expectedTimeSpan = TimeSpan.FromMinutes(5);

        _mockTimeEstimator.Setup(x => x.EstimateProcessingTime(files))
            .Returns(expectedTimeSpan);

        // Act
        var result = _processor.GetEstimatedProcessingTime(files);

        // Assert
        Assert.That(result, Is.EqualTo(expectedTimeSpan));
        _mockTimeEstimator.Verify(x => x.EstimateProcessingTime(files), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_ExecutesScriptWithCorrectArguments()
    {
        // Arrange
        var arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "a", Value = true },
            new BoolArgument { Flag = "s", Value = false }
        };

        // Setup mock to accept any arguments and return success
        // This is more flexible than trying to match the exact string
        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success output", ""));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Output, Is.EqualTo("Success output"));
        Assert.That(result.Error, Is.Empty);

        // Verify the script executor was called with the right executable
        _mockScriptExecutor.Verify(x => x.ExecuteScriptAsync(
            "Rscript",
            It.Is<string>(s => s.Contains("/path/to/script.R") &&
                               s.Contains("-a TRUE") &&
                               s.Contains("-s FALSE")),
            "/path/to",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_WhenScriptExecutorFails_ReturnsFalseWithError()
    {
        // Arrange
        var arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "a", Value = true }
        };

        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "", "Script execution failed"));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Script execution failed"));
    }

    [Test]
    public async Task ProcessAsync_WithCancellation_ReturnsFailureResult()
    {
        // Arrange
        var arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "a", Value = true }
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _processor.ProcessAsync(arguments, cancellationTokenSource.Token);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Output, Is.EqualTo("Operation was cancelled"));
        Assert.That(result.Error, Is.EqualTo("Processing cancelled by user"));
    }

    [Test]
    public async Task ProcessAsync_WithException_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var arguments = new List<ScriptArgument>
        {
            new BoolArgument { Flag = "a", Value = true }
        };

        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Output, Is.Empty);
        Assert.That(result.Error, Does.Contain("Failed to execute R script"));
        Assert.That(result.Error, Does.Contain("Test exception"));
    }

    [Test]
    public async Task ProcessAsync_WithNullArguments_UsesDefaultArguments()
    {
        // Arrange
        List<ScriptArgument> arguments = null;

        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success", ""));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.True);

        // Verify that the script executor was called with a command line that includes
        // arguments corresponding to the default arguments
        _mockScriptExecutor.Verify(x => x.ExecuteScriptAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("-a TRUE") && s.Contains("-s TRUE")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_WithEmptyArguments_StillBuildsCommandLine()
    {
        // Arrange
        var arguments = new List<ScriptArgument>();

        _mockScriptExecutor.Setup(x => x.ExecuteScriptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, "Success", ""));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.True);

        // Verify that the script executor was called with a command line that has the basic structure
        // but doesn't include any arguments from the empty list
        _mockScriptExecutor.Verify(x => x.ExecuteScriptAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("/path/to/script.R") && s.Contains("-d")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}