using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Process;

public class ScriptExecutor : IScriptExecutor
{
    public async Task<(bool Success, string Output)> ExecuteScriptAsync(
        string scriptPath, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
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
            if (args.Data == null) return;
            try
            {
                outputBuilder.AppendLine(args.Data);
            }
            catch
            {
                // ignored
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data == null) return;
            try
            {
                outputBuilder.AppendLine(args.Data);
            }
            catch
            {
                // ignored
            }
        };

        cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited) process.Kill(true);
            }
            catch
            {
                // ignored
            }
        });

        try
        {
            process.Start();
        }

        catch (Exception e)
        {
            return (false, $"Failed to start process: {e.Message}");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }

        catch (OperationCanceledException)
        {
            throw new OperationCanceledException();
        }

        return (process.ExitCode == 0, outputBuilder.ToString());
    }
}