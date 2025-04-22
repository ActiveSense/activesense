using System;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using Moq;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.SensorTests;

/// <summary>
/// Tests for the GeneActivProcessor class using mocks to avoid executing the actual R script
/// </summary>
[TestFixture]
public class GeneActivProcessorTests
{
    // Creating a testable subclass of GeneActivProcessor to override the process execution
    private class TestableGeneActivProcessor : GeneActivProcessor
    {
        private readonly bool _shouldSucceed;
        private readonly string _mockOutput;
        private readonly string _mockError;
        private readonly Exception _exceptionToThrow;
        
        public string CapturedScriptPath { get; private set; }
        public string CapturedArguments { get; private set; }
        public string CapturedWorkingDirectory { get; private set; }

        public TestableGeneActivProcessor(
            IScriptService scriptService,
            bool shouldSucceed = true, 
            string mockOutput = "", 
            string mockError = "",
            Exception exceptionToThrow = null) : base(scriptService)
        {
            _shouldSucceed = shouldSucceed;
            _mockOutput = mockOutput;
            _mockError = mockError;
            _exceptionToThrow = exceptionToThrow;
        }

        // Override the process execution to avoid actually running the script
        protected override Task<(bool Success, string Output, string Error)> ExecuteProcessAsync(
            string scriptPath, string arguments, string workingDirectory)
        {
            // Capture parameters for testing
            CapturedScriptPath = scriptPath;
            CapturedArguments = arguments;
            CapturedWorkingDirectory = workingDirectory;

            // If an exception is specified, throw it
            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }
            
            // Return mock results without actually running a process
            return Task.FromResult((_shouldSucceed, _mockOutput, _mockError));
        }
    }

    private Mock<IScriptService> _mockScriptService;
    private TestableGeneActivProcessor _processor;

    [SetUp]
    public void Setup()
    {
        // Create a mock of the IScriptService
        _mockScriptService = new Mock<IScriptService>();
        
        // Setup default mock responses
        _mockScriptService.Setup(s => s.GetExecutablePath()).Returns("mock-rscript");
        _mockScriptService.Setup(s => s.GetScriptPath()).Returns("/mock/path/_main.R");
        _mockScriptService.Setup(s => s.GetScriptBasePath()).Returns("/mock/path");
        _mockScriptService.Setup(s => s.GetScriptInputPath()).Returns("/mock/path/data");
        _mockScriptService.Setup(s => s.GetScriptOutputPath()).Returns("/mock/path/outputs");
    }

    [Test]
    public void SupportedType_ShouldReturnGeneActiv()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(_mockScriptService.Object);
        
        // Act & Assert
        Assert.That(_processor.SupportedType, Is.EqualTo(SensorTypes.GENEActiv));
    }

    [Test]
    public void SupportedFileTypes_ShouldIncludeCsvAndBin()
    {
        // Act & Assert
        var supportedTypes = GeneActivProcessor.SupportedFileTypes;
        Assert.That(supportedTypes, Contains.Item(".csv"));
        Assert.That(supportedTypes, Contains.Item(".bin"));
    }

    [Test]
    public async Task ProcessAsync_WhenProcessSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(
            _mockScriptService.Object,
            shouldSucceed: true,
            mockOutput: "Mock success output",
            mockError: "");

        // Act
        var result = await _processor.ProcessAsync("-d /test/output");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Output, Is.EqualTo("Mock success output"));
        Assert.That(result.Error, Is.Empty);

        // Verify script service methods were called
        _mockScriptService.Verify(s => s.GetScriptPath(), Times.Once);
        _mockScriptService.Verify(s => s.GetExecutablePath(), Times.Once);
        _mockScriptService.Verify(s => s.GetScriptBasePath(), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_WhenProcessFails_ReturnsFailureResult()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(
            _mockScriptService.Object,
            shouldSucceed: false,
            mockOutput: "Mock output",
            mockError: "Mock error message");

        // Act
        var result = await _processor.ProcessAsync("-d /test/output");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Output, Is.EqualTo("Mock output"));
        Assert.That(result.Error, Is.EqualTo("Mock error message"));
    }

    [Test]
    public async Task ProcessAsync_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(
            _mockScriptService.Object,
            exceptionToThrow: new Exception("Mock process exception"));

        // Act
        var result = await _processor.ProcessAsync("-d /test/output");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Output, Is.Empty);
        Assert.That(result.Error, Does.Contain("Failed to execute R script"));
        Assert.That(result.Error, Does.Contain("Mock process exception"));
    }

    [Test]
    public async Task ProcessAsync_UsesCorrectArguments()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(_mockScriptService.Object);
        var testArguments = "-d /test/specific/path";

        // Act
        await _processor.ProcessAsync(testArguments);

        // Assert
        Assert.That(_processor.CapturedScriptPath, Is.EqualTo("mock-rscript"));
        Assert.That(_processor.CapturedArguments, Does.Contain("/mock/path/_main.R"));
        Assert.That(_processor.CapturedArguments, Does.Contain(testArguments));
        Assert.That(_processor.CapturedWorkingDirectory, Is.EqualTo("/mock/path"));
    }
    
    [Test]
    public async Task ProcessAsync_WithEmptyArguments_ExecutesScriptWithEmptyArgString()
    {
        // Arrange
        _processor = new TestableGeneActivProcessor(_mockScriptService.Object);
        
        // Act
        await _processor.ProcessAsync();
        
        // Assert
        Assert.That(_processor.CapturedScriptPath, Is.EqualTo("mock-rscript"));
        Assert.That(_processor.CapturedArguments, Does.Contain("/mock/path/_main.R"));
        Assert.That(_processor.CapturedArguments, Does.Not.Contain("-d"));
    }
}