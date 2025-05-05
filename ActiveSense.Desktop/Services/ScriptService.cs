using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ActiveSense.Desktop.Services;

public interface IScriptService
{
    // Task<(bool Success, string Output, string Error)> ExecuteScriptAsync(
    //     string scriptPath, string workingDirectory, string arguments = "");

    string GetExecutablePath();

    string GetScriptBasePath();
    string GetScriptInputPath();
    string GetScriptOutputPath();
    string GetScriptPath();
}

public class RScriptService : IScriptService
{
    public string GetScriptBasePath()
    {
        // Navigate up one directory from the base directory to get to the solution directory
        // and then to the ActiveSense.RScripts directory
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string? solutionDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;

        if (solutionDirectory == null)
        {
            throw new DirectoryNotFoundException("Could not determine solution directory");
        }

        return Path.Combine(solutionDirectory, "ActiveSense.RScripts");
    }

    public string GetScriptPath()
    {
        return Path.Combine(GetScriptBasePath(), "_main.R");
    }

    public string GetScriptInputPath()
    {
        var path = Path.Combine(GetScriptBasePath(), "data");
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetScriptOutputPath()
    {
        var path = Path.Combine(GetScriptBasePath(), "outputs");
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetExecutablePath()
    {
        return "Rscript";
    }

}