using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Process;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using Moq;
using NUnit.Framework;
using Serilog;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ProcessTests;

[TestFixture]
public class GeneActiveProcessorTests
{
    [SetUp]
    public void Setup()
    {
        _mockPathService = new Mock<IPathService>();
        _mockScriptExecutor = new Mock<IScriptExecutor>();
        _mockFileManager = new Mock<IFileManager>();
        _mockTimeEstimator = new Mock<IProcessingTimeEstimator>();
        _mockLogger = new Mock<ILogger>();

        _processor = new GeneActiveProcessor(
            _mockPathService.Object,
            _mockScriptExecutor.Object,
            _mockFileManager.Object,
            _mockTimeEstimator.Object,
            _mockLogger.Object);

        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);

        // Set up the path service mock
        _mockPathService.Setup(x => x.MainScriptPath).Returns("/path/to/script.R");
        _mockPathService.Setup(x => x.ScriptExecutablePath).Returns("Rscript");
        _mockPathService.Setup(x => x.ScriptBasePath).Returns("/path/to");
        _mockPathService.Setup(x => x.OutputDirectory).Returns("/path/to/outputs");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    private Mock<IPathService> _mockPathService;
    private Mock<IScriptExecutor> _mockScriptExecutor;
    private Mock<IFileManager> _mockFileManager;
    private Mock<IProcessingTimeEstimator> _mockTimeEstimator;
    private GeneActiveProcessor _processor;
    private string _tempDir;
    private Mock<ILogger> _mockLogger;

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
        Assert.That(args.Count, Is.EqualTo(12));

        // Check for activity analysis argument
        var activityArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "activity");
        Assert.That(activityArg, Is.Not.Null);
        Assert.That((activityArg as BoolArgument)?.Value, Is.True);

        // Check for sleep analysis argument
        var sleepArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "sleep");
        Assert.That(sleepArg, Is.Not.Null);
        Assert.That((sleepArg as BoolArgument)?.Value, Is.True);

        // Check for sleep analysis argument
        var legacyArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "legacy");
        Assert.That(legacyArg, Is.Not.Null);
        Assert.That((legacyArg as BoolArgument)?.Value, Is.False);

        // Check for sleep analysis argument
        var clippingArg = args.FirstOrDefault(a => a is BoolArgument arg && arg.Flag == "clipping");
        Assert.That(clippingArg, Is.Not.Null);
        Assert.That((clippingArg as BoolArgument)?.Value, Is.True);

        // Check for left wrist thresholds
        var sedentaryLeftArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "sedentary_left");
        Assert.That(sedentaryLeftArg, Is.Not.Null);
        Assert.That((sedentaryLeftArg as NumericArgument)?.Value, Is.EqualTo(0.04));

        var lightLeftArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "light_left");
        Assert.That(lightLeftArg, Is.Not.Null);
        Assert.That((lightLeftArg as NumericArgument)?.Value, Is.EqualTo(217));

        var moderateLeftArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "moderate_left");
        Assert.That(moderateLeftArg, Is.Not.Null);
        Assert.That((moderateLeftArg as NumericArgument)?.Value, Is.EqualTo(644));

        var vigorousLeftArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "vigorous_left");
        Assert.That(vigorousLeftArg, Is.Not.Null);
        Assert.That((vigorousLeftArg as NumericArgument)?.Value, Is.EqualTo(1810));

        // Check for right wrist thresholds
        var sedentaryRightArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "sedentary_right");
        Assert.That(sedentaryRightArg, Is.Not.Null);
        Assert.That((sedentaryRightArg as NumericArgument)?.Value, Is.EqualTo(0.04));

        var lightRightArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "light_right");
        Assert.That(lightRightArg, Is.Not.Null);
        Assert.That((lightRightArg as NumericArgument)?.Value, Is.EqualTo(386));

        var moderateRightArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "moderate_right");
        Assert.That(moderateRightArg, Is.Not.Null);
        Assert.That((moderateRightArg as NumericArgument)?.Value, Is.EqualTo(439));

        var vigorousRightArg = args.FirstOrDefault(a => a is NumericArgument arg && arg.Flag == "vigorous_right");
        Assert.That(vigorousRightArg, Is.Not.Null);
        Assert.That((vigorousRightArg as NumericArgument)?.Value, Is.EqualTo(2098));
    }

    [Test]
    public async Task CopyFiles_DelegatesCallToFileManager()
    {
        // Arrange
        string[] files = { "file1.bin", "file2.bin" };
        var processingDir = "/path/to/processing";
        var outputDir = "/path/to/output";

        // Act
        await _processor.CopyFilesAsync(files, processingDir, outputDir);

        // Assert
        _mockFileManager.Verify(x => x.CopyFiles(
                files,
                processingDir,
                outputDir,
                It.Is<string[]>(types => types.Contains(".bin"))),
            Times.Once);
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
            .ReturnsAsync((true, "Success output"));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Output, Is.EqualTo("Success output"));

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
            .ReturnsAsync((false, "Script execution failed"));

        // Act
        var result = await _processor.ProcessAsync(arguments);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Output, Is.EqualTo("Script execution failed"));
    }

    [Test]
    public Task ProcessAsync_WithCancellation_Throws()
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

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await _processor.ProcessAsync(arguments, cancellationTokenSource.Token);
        });
        return Task.CompletedTask;
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
        Assert.That(result.Output, Does.Contain("Failed to execute R script"));
        Assert.That(result.Output, Does.Contain("Test exception"));
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
            .ReturnsAsync((true, "Success"));

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