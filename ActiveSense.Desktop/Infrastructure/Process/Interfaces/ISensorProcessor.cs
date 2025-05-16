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
    Task<(bool Success, string Output)> ProcessAsync(IEnumerable<ScriptArgument> arguments, CancellationToken cancellationToken);
    TimeSpan GetEstimatedProcessingTime(IEnumerable<string> files);
    string ProcessingInfo { get; }
    void CopyFiles(string[] files, string processingDirectory, string outputDirectory);
}