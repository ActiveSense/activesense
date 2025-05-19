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

public class GeneActiveProcessor : ISensorProcessor
{
    private readonly List<ScriptArgument> _defaultArguments;
    private readonly IFileManager _fileManager;
    private readonly IPathService _pathService;
    private readonly IScriptExecutor _scriptExecutor;
    private readonly IProcessingTimeEstimator _timeEstimator;
    Serilog.ILogger _logger; 

    public GeneActiveProcessor(
        IPathService pathService,
        IScriptExecutor scriptExecutor,
        IFileManager fileManager,
        IProcessingTimeEstimator timeEstimator,
        Serilog.ILogger logger)
    {
        _logger = logger;
        _pathService = pathService;
        _scriptExecutor = scriptExecutor;
        _fileManager = fileManager;
        _timeEstimator = timeEstimator;
        _defaultArguments = CreateDefaultArguments();
    }

    private static string[] SupportedFileTypes => [".bin"];

    public SensorTypes SupportedType => SensorTypes.GENEActiv;

    public IReadOnlyList<ScriptArgument> DefaultArguments => _defaultArguments;

    public async Task<(bool Success, string Output)> ProcessAsync(
        IEnumerable<ScriptArgument> arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptPath = _pathService.MainScriptPath;
            var executablePath = _pathService.ScriptExecutablePath;
            var workingDirectory = _pathService.ScriptBasePath;
            
            _logger.Information("Starting GeneActive processing");
            _logger.Information($"Script path: {scriptPath}");
            _logger.Information($"Executable path: {executablePath}");
            _logger.Information($"Working directory: {workingDirectory}");

            var argsToUse = arguments?.ToList() ?? _defaultArguments;

            var outputDir = $"-d \"{_pathService.OutputDirectory}\"";

            var scriptArguments = string.Join(" ",
                argsToUse
                    .Select(arg => arg.ToCommandLineArgument())
                    .Where(arg => !string.IsNullOrEmpty(arg)));

            var processArguments = $"\"{scriptPath}\" {outputDir} {scriptArguments}";

            var result = await _scriptExecutor.ExecuteScriptAsync(executablePath, processArguments, workingDirectory,
                cancellationToken);
            
            _logger.Information("Processing completed");
            
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            _logger.Error("R executable not found.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message, "Error executing R script");
            return (false, $"Failed to execute R script: {ex.Message}");
        }
    }

    public void CopyFiles(string[] files, string processingDirectory, string outputDirectory)
    {
        _fileManager.CopyFiles(files, processingDirectory, outputDirectory, SupportedFileTypes);
    }

    public TimeSpan GetEstimatedProcessingTime(IEnumerable<string> files)
    {
        return _timeEstimator.EstimateProcessingTime(files);
    }

    public string ProcessingInfo =>
        "Einstellungen werden nur auf die Verarbeitung von .bin-Dateien angewendet. Beim Import als PDF werden die Einstellungen ignoriert.";

    private List<ScriptArgument> CreateDefaultArguments()
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
                Description = "Threshold for sedentary activity on left wrist (in g)",
                MinValue = 0.01,
                MaxValue = 0.1,
                Value = 0.04
            },

            new NumericArgument
            {
                Flag = "light_left",
                Name = "Light Threshold (Left)",
                Description = "Threshold for light activity on left wrist",
                MinValue = 100,
                MaxValue = 500,
                Value = 217
            },

            new NumericArgument
            {
                Flag = "moderate_left",
                Name = "Moderate Threshold (Left)",
                Description = "Threshold for moderate activity on left wrist",
                MinValue = 300,
                MaxValue = 1000,
                Value = 644
            },

            new NumericArgument
            {
                Flag = "vigorous_left",
                Name = "Vigorous Threshold (Left)",
                Description = "Threshold for vigorous activity on left wrist",
                MinValue = 1000,
                MaxValue = 3000,
                Value = 1810
            },

            // Right wrist thresholds
            new NumericArgument
            {
                Flag = "sedentary_right",
                Name = "Sedentary Threshold (Right)",
                Description = "Threshold for sedentary activity on right wrist (in g)",
                MinValue = 0.01,
                MaxValue = 0.1,
                Value = 0.04
            },

            new NumericArgument
            {
                Flag = "light_right",
                Name = "Light Threshold (Right)",
                Description = "Threshold for light activity on right wrist",
                MinValue = 100,
                MaxValue = 800,
                Value = 386
            },

            new NumericArgument
            {
                Flag = "moderate_right",
                Name = "Moderate Threshold (Right)",
                Description = "Threshold for moderate activity on right wrist",
                MinValue = 200,
                MaxValue = 800,
                Value = 439
            },

            new NumericArgument
            {
                Flag = "vigorous_right",
                Name = "Vigorous Threshold (Right)",
                Description = "Threshold for vigorous activity on right wrist",
                MinValue = 1000,
                MaxValue = 3500,
                Value = 2098
            }
        ];
    }
}