using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ActiveSense.Desktop.Services;

public interface IRScriptService
{
    Task<(bool Success, string Output, string Error)> ExecuteScriptAsync(
        string scriptPath, string workingDirectory, string arguments = "");

    string GetRScriptBasePath();
    string GetRDataPath();
    string GetROutputPath();
}

public class RScriptService : IRScriptService
{
    private readonly string _rExecutablePath;
    
    public RScriptService(string rExecutablePath = "Rscript.exe")
    {
        _rExecutablePath = rExecutablePath;
    }
    
    public async Task<(bool Success, string Output, string Error)> ExecuteScriptAsync(string scriptPath,
        string workingDirectory, string arguments = "")
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _rExecutablePath,
                    Arguments = $"\"{scriptPath}\" {arguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
                
            await process.WaitForExitAsync();
                
            string output = await outputTask;
            string error = await errorTask;

            return (process.ExitCode == 0, output, error);
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Failed to execute R script: {ex.Message}");
        }
    }

    public string GetRScriptBasePath()
    {
        // Navigate up one directory from the base directory to get to the solution directory
        // and then to the ActiveSense.RScripts directory
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string solutionDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        
        if (solutionDirectory == null)
        {
            throw new DirectoryNotFoundException("Could not determine solution directory");
        }
        
        return Path.Combine(solutionDirectory, "ActiveSense.RScripts");
    }

    public string GetRDataPath()
    {
        var path = Path.Combine(GetRScriptBasePath(), "data");
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetROutputPath()
    {
        var path = Path.Combine(GetRScriptBasePath(), "outputs");
        Directory.CreateDirectory(path);
        return path;
    }
    
}