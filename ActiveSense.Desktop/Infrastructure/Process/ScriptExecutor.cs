using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Process;

public class ScriptExecutor : IScriptExecutor
{
    public async Task<(bool Success, string Output, string Error)> ExecuteScriptAsync(
        string scriptPath, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Executing script: " + scriptPath);
        Console.WriteLine("Arguments: " + arguments);
        Console.WriteLine("Working Directory: " + workingDirectory);
        
        var process = new System.Diagnostics.Process
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

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            return (false, outputBuilder.ToString(), $"Failed to start process: {e.Message}");
        }
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
}
