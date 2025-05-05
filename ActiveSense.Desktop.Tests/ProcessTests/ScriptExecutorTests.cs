using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Process.Implementations;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.ProcessTests;

[TestFixture]
public class ScriptExecutorTests
{
    private ScriptExecutor _scriptExecutor;
    private string _tempDir;
    
    [SetUp]
    public void Setup()
    {
        _scriptExecutor = new ScriptExecutor();
        
        // Create temp directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }
    
    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch (IOException)
            {
                // Files might be locked, try to delete what we can
                Console.WriteLine("Warning: Could not completely clean up temp directory");
            }
        }
    }
    
    // These tests depend on the operating system and available executables
    // We'll use a simple command that should work across platforms

    [Test]
    public async Task ExecuteScriptAsync_WithValidCommand_ReturnsSuccessAndOutput()
    {
        // Arrange
        string scriptPath = GetCrossPlatformCommand();
        string arguments = GetCrossPlatformArguments();
        
        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            scriptPath, 
            arguments, 
            _tempDir);
        
        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Output, Is.Not.Empty);
        Assert.That(result.Error, Is.Empty);
    }
    
    [Test]
    public async Task ExecuteScriptAsync_WithInvalidCommand_ReturnsFalseAndError()
    {
        // Arrange
        string invalidScriptPath = Path.Combine(_tempDir, "nonexistent_script");
        
        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            invalidScriptPath, 
            "", 
            _tempDir);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Empty);
    }
    
    [Test]
    public async Task ExecuteScriptAsync_WithCancellationToken_CancelsExecution()
    {
        // Arrange
        string scriptPath = GetSleepCommand();
        string arguments = GetSleepArguments();
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        // Act
        var executionTask = _scriptExecutor.ExecuteScriptAsync(
            scriptPath, 
            arguments, 
            _tempDir, 
            cancellationTokenSource.Token);
        
        // Wait a moment then cancel
        await Task.Delay(100);
        cancellationTokenSource.Cancel();
        
        var result = await executionTask;
        
        // Assert
        Assert.That(result.Success, Is.False);
    }
    
    [Test]
    public async Task ExecuteScriptAsync_WithOutputAndErrorStreams_CapturesBoth()
    {
        // Arrange
        string scriptPath = GetEchoToErrorCommand();
        string arguments = GetEchoToErrorArguments("This is standard output", "This is error output");
        
        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            scriptPath, 
            arguments, 
            _tempDir);
        
        // Assert
        Assert.That(result.Output, Does.Contain("standard output"));
        Assert.That(result.Error, Does.Contain("error output"));
    }
    
    [Test]
    public async Task ExecuteScriptAsync_WithLongRunningProcess_CapturesAllOutput()
    {
        // Arrange
        string scriptPath = GetRepeatedOutputCommand();
        string arguments = GetRepeatedOutputArguments();
        
        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            scriptPath, 
            arguments, 
            _tempDir);
        
        // Assert
        Assert.That(result.Success, Is.True);
        
        // Count the number of lines in the output
        var outputLines = result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(outputLines.Length, Is.GreaterThanOrEqualTo(10));
    }

    #region Helper Methods

    private string GetCrossPlatformCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return "cmd.exe";
        }
        else
        {
            return "echo";
        }
    }

    private string GetCrossPlatformArguments()
    {
        if (OperatingSystem.IsWindows())
        {
            return "/c echo Hello, World!";
        }
        else
        {
            return "Hello, World!";
        }
    }
    
    private string GetSleepCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return "cmd.exe";
        }
        else
        {
            return "sleep";
        }
    }
    
    private string GetSleepArguments()
    {
        if (OperatingSystem.IsWindows())
        {
            return "/c timeout 10";
        }
        else
        {
            return "10";
        }
    }
    
    private string GetEchoToErrorCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return "cmd.exe";
        }
        else
        {
            return "bash";
        }
    }
    
    private string GetEchoToErrorArguments(string stdout, string stderr)
    {
        if (OperatingSystem.IsWindows())
        {
            return $"/c \"echo {stdout} && echo {stderr} 1>&2\"";
        }
        else
        {
            return $"-c \"echo '{stdout}' && echo '{stderr}' 1>&2\"";
        }
    }
    
    private string GetRepeatedOutputCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            return "cmd.exe";
        }
        else
        {
            return "bash";
        }
    }
    
    private string GetRepeatedOutputArguments()
    {
        if (OperatingSystem.IsWindows())
        {
            return "/c \"for /L %i in (1,1,10) do @(echo Line %i && timeout /T 1 /NOBREAK > nul)\"";
        }
        else
        {
            return "-c \"for i in {1..10}; do echo Line $i; sleep 0.1; done\"";
        }
    }
    
    #endregion
}