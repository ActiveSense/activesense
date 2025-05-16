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

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                try
                {
                    outputBuilder.AppendLine(args.Data);
                }
                catch {}
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                try
                {
                    outputBuilder.AppendLine(args.Data);
                }
                catch {}
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
            return (false, $"Failed to start process: {e.Message}", string.Empty);
        }
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Return what we have if cancelled
            return (false, outputBuilder.ToString(), string.Empty);
        }

        return (process.ExitCode == 0, outputBuilder.ToString(), string.Empty);
    }
}