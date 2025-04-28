using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Interfaces;

public interface ISensorProcessor
{
    SensorTypes SupportedType { get; }
    IReadOnlyList<ScriptArgument> DefaultArguments { get; }
    Task<(bool Success, string Output, string Error)> ProcessAsync(IEnumerable<ScriptArgument> arguments);
    void CopyFiles(string[] files, string processingDirectory, string outputDirectory);
}