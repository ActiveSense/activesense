using System.Threading;
using System.Threading.Tasks;

namespace ActiveSense.Desktop.Infrastructure.Process.Interfaces;


public interface IScriptExecutor
{
    Task<(bool Success, string Output)> ExecuteScriptAsync(
        string scriptPath, string arguments, string workingDirectory, CancellationToken cancellationToken);
}