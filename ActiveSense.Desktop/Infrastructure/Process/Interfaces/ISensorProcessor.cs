using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;

namespace ActiveSense.Desktop.Infrastructure.Process.Interfaces;

public interface ISensorProcessor
{
    SensorTypes SupportedType { get; }
    IReadOnlyList<ScriptArgument> DefaultArguments { get; }
    Task<(bool Success, string Output)> ProcessAsync(IList<ScriptArgument> arguments, CancellationToken cancellationToken);
    Task<TimeSpan> GetEstimatedProcessingTimeAsync(IEnumerable<string> files, IList<ScriptArgument> arguments);
    string ProcessingInfo { get; }
    Task CopyFilesAsync(string[] files, string processingDirectory, string outputDirectory);
}