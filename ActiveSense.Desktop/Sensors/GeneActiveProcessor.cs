using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;

namespace ActiveSense.Desktop.Sensors;

public class GeneActiveProcessor : ISensorProcessor
{
    private readonly List<ScriptArgument> _defaultArguments;
    private readonly IScriptService _rScriptService;

    public GeneActiveProcessor(IScriptService rScriptService)
    {
        _rScriptService = rScriptService ?? new RScriptService();
        _defaultArguments = CreateDefaultArguments();
    }

    public static string[] SupportedFileTypes => [".csv", ".bin"];

    public SensorTypes SupportedType => SensorTypes.GENEActiv;
    public IReadOnlyList<ScriptArgument> DefaultArguments => _defaultArguments;

    public async Task<(bool Success, string Output, string Error)> ProcessAsync(
        IEnumerable<ScriptArgument> arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptPath = _rScriptService.GetScriptPath();
            var executablePath = _rScriptService.GetExecutablePath();
            var workingDirectory = _rScriptService.GetScriptBasePath();

            var argsToUse = arguments?.ToList() ?? _defaultArguments;

            var outputDir = $"-d \"{AppConfig.OutputsDirectoryPath}\"";

            var scriptArguments = string.Join(" ",
                argsToUse
                    .Select(arg => arg.ToCommandLineArgument())
                    .Where(arg => !string.IsNullOrEmpty(arg)));

            var processArguments = $"\"{scriptPath}\" {outputDir} {scriptArguments}";

            return await ExecuteProcessAsync(executablePath, processArguments, workingDirectory, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return (false, "Operation was cancelled", "Processing cancelled by user");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Failed to execute R script: {ex.Message}");
        }
    }

    public void CopyFiles(string[] files, string processingDirectory, string outputDirectory)
    {
        if (files == null || files.Length == 0) return;

        ClearProcessingDirectory(processingDirectory);
        Directory.CreateDirectory(processingDirectory);
        Directory.CreateDirectory(outputDirectory);

        foreach (var file in files)
            try
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                var fileName = Path.GetFileName(file);

                if (extension == ".bin")
                {
                    var destinationPath = Path.Combine(processingDirectory, fileName);
                    File.Copy(file, destinationPath, true);
                }
                else if (extension == ".pdf")
                {
                    var destinationPath = Path.Combine(outputDirectory, fileName);
                    File.Copy(file, destinationPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file {file}: {ex.Message}");
            }
    }

    public TimeSpan GetEstimatedProcessingTime(IEnumerable<string> files)
    {
        if (files == null || !files.Any())
            return TimeSpan.Zero;

        var fileCount = files.Count();
        long totalSize = 0;

        foreach (var file in files)
            if (File.Exists(file))
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

        double estimatedSeconds = totalSize / (1024 * 1024) * 6;

        return TimeSpan.FromSeconds(Math.Max(5, estimatedSeconds));
    }

    private List<ScriptArgument> CreateDefaultArguments()
    {
        return
        [
            new BoolArgument
            {
                Flag = "a",
                Name = "Activity Analysis",
                Description = "Run activity analysis",
                Value = true
            },

            new BoolArgument
            {
                Flag = "s",
                Name = "Sleep Analysis",
                Description = "Run sleep analysis",
                Value = true
            }
        ];
    }

    protected virtual async Task<(bool Success, string Output, string Error)> ExecuteProcessAsync(
        string scriptPath, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = scriptPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            },
            EnableRaisingEvents = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                outputBuilder.AppendLine(args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                errorBuilder.AppendLine(args.Data);
        };

        // Register for cancellation
        cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited) process.Kill(true);
            }
            catch
            {
                /* Ignore errors during cancellation */
            }
        });

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return (false, outputBuilder.ToString(), "Operation was cancelled");
        }

        return (process.ExitCode == 0, outputBuilder.ToString(), errorBuilder.ToString());
    }

    public void ClearProcessingDirectory(string processingDirectory)
    {
        if (Directory.Exists(processingDirectory))
            try
            {
                Directory.Delete(processingDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing processing directory: {ex.Message}");
            }
    }
}