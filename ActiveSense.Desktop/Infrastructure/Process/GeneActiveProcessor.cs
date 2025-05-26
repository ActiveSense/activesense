using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using Serilog;

namespace ActiveSense.Desktop.Infrastructure.Process;

public class GeneActiveProcessor(
    IPathService pathService,
    IScriptExecutor scriptExecutor,
    IFileManager fileManager,
    IProcessingTimeEstimator timeEstimator,
    ILogger logger)
    : ISensorProcessor
{
    private readonly List<ScriptArgument> _defaultArguments = CreateDefaultArguments();

    private static string[] SupportedFileTypes => [".bin"];

    public SensorTypes SupportedType => SensorTypes.GENEActiv;

    public IReadOnlyList<ScriptArgument> DefaultArguments => _defaultArguments;

    public async Task<(bool Success, string Output)> ProcessAsync(
        IList<ScriptArgument> arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptPath = pathService.MainScriptPath;
            var executablePath = pathService.ScriptExecutablePath;
            var workingDirectory = pathService.ScriptBasePath;

            logger.Information("Processing started");
            logger.Information($"Script path: {scriptPath}");
            logger.Information($"Executable path: {executablePath}");
            logger.Information($"Working directory: {workingDirectory}");
            logger.Information("Processing Arguments: {Arguments}",
                string.Join(", ", arguments.Select(a => a.ToCommandLineArgument())));

            var argsToUse = arguments;

            var outputDir = $"-d \"{pathService.OutputDirectory}\"";

            var scriptArguments = string.Join(" ",
                argsToUse
                    .Select(arg => arg.ToCommandLineArgument())
                    .Where(arg => !string.IsNullOrEmpty(arg)));

            var processArguments = $"\"{scriptPath}\" {outputDir} {scriptArguments}";

            var result = await scriptExecutor.ExecuteScriptAsync(executablePath, processArguments, workingDirectory,
                cancellationToken);

            logger.Information("Processing completed");

            return result;
        }

        catch (OperationCanceledException)
        {
            throw;
        }

        catch (FileNotFoundException)
        {
            logger.Error("R executable not found.");
            throw;
        }

        catch (Exception ex)
        {
            logger.Error(ex.Message, "Error executing R script");
            return (false, $"Failed to execute R script: {ex.Message}");
        }
    }

    public async Task<TimeSpan> GetEstimatedProcessingTimeAsync(IEnumerable<string> files, IList<ScriptArgument> arguments)
    {
        long totalSizeBytes = 0;
        foreach (var filePath in files)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) continue;
            try
            {
                totalSizeBytes += new FileInfo(filePath).Length;
            }
            catch (Exception ex)
            {
                logger.Warning("Could not get size for file {filePath}. Error: {error}", filePath, ex.Message);
            }
        }

        var totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);

        return await Task.Run(() => timeEstimator.EstimateProcessingTime(totalSizeMB, arguments));
    }

    public async Task CopyFilesAsync(string[] files, string processingDirectory, string outputDirectory)
    {
        await Task.Run(() => fileManager.CopyFiles(files, processingDirectory, outputDirectory, SupportedFileTypes));
    }

    public string ProcessingInfo =>
        "Einstellungen werden nur auf die Verarbeitung von .bin-Dateien angewendet. Beim Import als PDF werden die Einstellungen ignoriert.";

    #region Arguments

    private static List<ScriptArgument> CreateDefaultArguments()
    {
        return
        [
            new BoolArgument
            {
                Flag = "activity",
                Name = "Activity Analysis",
                Description = "Run activity analysis",
                Value = true
            },

            new BoolArgument
            {
                Flag = "sleep",
                Name = "Sleep Analysis",
                Description = "Run sleep analysis",
                Value = true
            },

            // Left wrist thresholds
            new NumericArgument
            {
                Flag = "sedentary_left",
                Name = "Sedentary Threshold (Left)",
                Description = "Threshold for sedentary activity on left wrist in g",
                MinValue = 0.01,
                MaxValue = 0.1,
                Value = 0.04
            },

            new NumericArgument
            {
                Flag = "light_left",
                Name = "Light Threshold (Left)",
                Description = "Threshold for light activity on left wrist in g",
                MinValue = 100,
                MaxValue = 500,
                Value = 217
            },

            new NumericArgument
            {
                Flag = "moderate_left",
                Name = "Moderate Threshold (Left)",
                Description = "Threshold for moderate activity on left wrist in g",
                MinValue = 300,
                MaxValue = 1000,
                Value = 644
            },

            new NumericArgument
            {
                Flag = "vigorous_left",
                Name = "Vigorous Threshold (Left)",
                Description = "Threshold for vigorous activity on left wrist in g",
                MinValue = 1000,
                MaxValue = 3000,
                Value = 1810
            },

            // Right wrist thresholds
            new NumericArgument
            {
                Flag = "sedentary_right",
                Name = "Sedentary Threshold (Right)",
                Description = "Threshold for sedentary activity on right wrist in g",
                MinValue = 0.01,
                MaxValue = 0.1,
                Value = 0.04
            },

            new NumericArgument
            {
                Flag = "light_right",
                Name = "Light Threshold (Right)",
                Description = "Threshold for light activity on right wrist in g",
                MinValue = 100,
                MaxValue = 800,
                Value = 386
            },

            new NumericArgument
            {
                Flag = "moderate_right",
                Name = "Moderate Threshold (Right)",
                Description = "Threshold for moderate activity on right wrist in g",
                MinValue = 200,
                MaxValue = 800,
                Value = 439
            },

            new NumericArgument
            {
                Flag = "vigorous_right",
                Name = "Vigorous Threshold (Right)",
                Description = "Threshold for vigorous activity on right wrist in g",
                MinValue = 1000,
                MaxValue = 3500,
                Value = 2098
            }
        ];
    }

    #endregion
}