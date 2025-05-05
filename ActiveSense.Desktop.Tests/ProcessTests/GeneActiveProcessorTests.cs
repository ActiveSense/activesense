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
    private Mock<IScriptService> _mockScriptService;
    private Mock<IScriptExecutor> _mockScriptExecutor;
    private Mock<IFileManager> _mockFileManager;
    private Mock<IProcessingTimeEstimator> _mockTimeEstimator;
    private GeneActiveProcessor _processor;
    private string _tempDir;

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
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

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
        string processingDir = "/path/to/processing";
        string outputDir = "/path/to/output";

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

}