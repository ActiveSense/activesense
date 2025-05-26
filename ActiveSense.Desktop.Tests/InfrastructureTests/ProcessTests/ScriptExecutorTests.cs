using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Infrastructure.Process;
using NUnit.Framework;

namespace ActiveSense.Desktop.Tests.InfrastructureTests.ProcessTests;

[TestFixture]
public class ScriptExecutorTests
{
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

    private ScriptExecutor _scriptExecutor;
    private string _tempDir;

    // These tests depend on the operating system and available executables
    // We'll use a simple command that should work across platforms

    [Test]
    public async Task ExecuteScriptAsync_WithValidCommand_ReturnsSuccessAndOutput()
    {
        // Arrange
        var scriptPath = GetCrossPlatformCommand();
        var arguments = GetCrossPlatformArguments();

        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            scriptPath,
            arguments,
            _tempDir);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Output, Is.Not.Empty);
    }

    [Test]
    public async Task ExecuteScriptAsync_WithInvalidCommand_ReturnsFalse()
    {
        // Arrange
        var invalidScriptPath = Path.Combine(_tempDir, "nonexistent_script");

        // Act
        var result = await _scriptExecutor.ExecuteScriptAsync(
            invalidScriptPath,
            "",
            _tempDir);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task ExecuteScriptAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var scriptPath = GetSleepCommand();
        var arguments = GetSleepArguments();

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

        // Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () => await executionTask);
    }

    [Test]
    public async Task ExecuteScriptAsync_WithLongRunningProcess_CapturesAllOutput()
    {
        // Arrange
        var scriptPath = GetRepeatedOutputCommand();
        var arguments = GetRepeatedOutputArguments();

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

    private string GetCrossPlatformCommand()
    {
        if (OperatingSystem.IsWindows()) return "cmd.exe";

        return "echo";
    }

    private string GetCrossPlatformArguments()
    {
        if (OperatingSystem.IsWindows()) return "/c echo Hello, World!";

        return "Hello, World!";
    }

    private string GetSleepCommand()
    {
        if (OperatingSystem.IsWindows()) return "cmd.exe";

        return "sleep";
    }

    private string GetSleepArguments()
    {
        if (OperatingSystem.IsWindows()) return "/c timeout 10";

        return "10";
    }

    private string GetEchoToErrorCommand()
    {
        if (OperatingSystem.IsWindows()) return "cmd.exe";

        return "bash";
    }

    private string GetEchoToErrorArguments(string stdout, string stderr)
    {
        if (OperatingSystem.IsWindows()) return $"/c \"echo {stdout} && echo {stderr} 1>&2\"";

        return $"-c \"echo '{stdout}' && echo '{stderr}' 1>&2\"";
    }

    private string GetRepeatedOutputCommand()
    {
        if (OperatingSystem.IsWindows()) return "cmd.exe";

        return "bash";
    }

    private string GetRepeatedOutputArguments()
    {
        if (OperatingSystem.IsWindows())
            return "/c \"for /L %i in (1,1,10) do @(echo Line %i && timeout /T 1 /NOBREAK > nul)\"";

        return "-c \"for i in {1..10}; do echo Line $i; sleep 0.1; done\"";
    }
}