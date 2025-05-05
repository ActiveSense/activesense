using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Process.Interfaces;
using ActiveSense.Desktop.Services;

namespace ActiveSense.Desktop.Process.Implementations;

public class GeneActiveProcessor : ISensorProcessor
{
    private readonly List<ScriptArgument> _defaultArguments;
    private readonly IScriptService _scriptService;
    private readonly IScriptExecutor _scriptExecutor;
    private readonly IFileManager _fileManager;
    private readonly IProcessingTimeEstimator _timeEstimator;

    public GeneActiveProcessor(
        IScriptService scriptService,
        IScriptExecutor scriptExecutor,
        IFileManager fileManager,
        IProcessingTimeEstimator timeEstimator)
    {
        _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
        _scriptExecutor = scriptExecutor ?? throw new ArgumentNullException(nameof(scriptExecutor));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _timeEstimator = timeEstimator ?? throw new ArgumentNullException(nameof(timeEstimator));
        _defaultArguments = CreateDefaultArguments();
    }

    private static string[] SupportedFileTypes => [".bin"];

    public SensorTypes SupportedType => SensorTypes.GENEActiv;

    public IReadOnlyList<ScriptArgument> DefaultArguments => _defaultArguments;

    public async Task<(bool Success, string Output, string Error)> ProcessAsync(
        IEnumerable<ScriptArgument> arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var scriptPath = _scriptService.GetScriptPath();
            var executablePath = _scriptService.GetExecutablePath();
            var workingDirectory = _scriptService.GetScriptBasePath();

            var argsToUse = arguments?.ToList() ?? _defaultArguments;

            var outputDir = $"-d \"{AppConfig.OutputsDirectoryPath}\"";

            var scriptArguments = string.Join(" ",
                argsToUse
                    .Select(arg => arg.ToCommandLineArgument())
                    .Where(arg => !string.IsNullOrEmpty(arg)));

            var processArguments = $"\"{scriptPath}\" {outputDir} {scriptArguments}";

            var result = await _scriptExecutor.ExecuteScriptAsync(executablePath, processArguments, workingDirectory, cancellationToken);
            return result;
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
        _fileManager.CopyFiles(files, processingDirectory, outputDirectory, SupportedFileTypes);
    }

    public TimeSpan GetEstimatedProcessingTime(IEnumerable<string> files)
    {
        return _timeEstimator.EstimateProcessingTime(files);
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
}