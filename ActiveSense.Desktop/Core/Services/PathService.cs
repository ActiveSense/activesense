using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ActiveSense.Desktop.Core.Services.Interfaces;

namespace ActiveSense.Desktop.Core.Services;

public class PathService : IPathService
{
    private readonly string _customOutputPath;
    private readonly string _customScriptPath;

    public PathService(string customOutputPath = null, string customScriptPath = null)
    {
        _customOutputPath = customOutputPath;
        _customScriptPath = customScriptPath;

        // Ensure critical directories exist on initialization
        EnsureDirectoryExists(OutputDirectory);
        EnsureDirectoryExists(ScriptInputPath);
    }

    // Base paths
    public string ApplicationBasePath => AppDomain.CurrentDomain.BaseDirectory;

    public string ScriptBasePath
    {
        get
        {
            if (_customScriptPath != null)
                return _customScriptPath;

            // Check relative to application first
            var relativePath = CombinePaths(SolutionBasePath, "../ActiveSense.RScripts");
            if (Directory.Exists(relativePath))
                return relativePath;

            // Fall back to user directory
            var userPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ActiveSense", "RScripts");

            if (!Directory.Exists(userPath))
            {
                EnsureDirectoryExists(userPath);
                CopyResourceScripts(userPath);
            }

            return userPath;
        }
    }

    public string SolutionBasePath
    {
        get
        {
            var directory = ApplicationBasePath;

            if (directory.Contains("bin"))
                while (!Directory.Exists(Path.Combine(directory, "ActiveSense.Desktop")) &&
                       !File.Exists(Path.Combine(directory, "ActiveSense.Desktop.sln")))
                {
                    var parentDir = Directory.GetParent(directory);
                    if (parentDir == null) return ApplicationBasePath;

                    directory = parentDir.FullName;
                }

            return directory;
        }
    }

    public string OutputDirectory => CombinePaths(SolutionBasePath, "AnalysisFiles/");

    // Script paths
    public string ScriptInputPath => CombinePaths(ScriptBasePath, "data");
    public string MainScriptPath => CombinePaths(ScriptBasePath, "_main.R");

    public string ScriptExecutablePath => FindRInstallation();

    // Utility methods
    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public bool EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return true;
        }

        return false;
    }

    public void ClearDirectory(string path)
    {
        if (Directory.Exists(path))
            try
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing directory: {ex.Message}");
            }
        else
            Directory.CreateDirectory(path);
    }

    private void CopyResourceScripts(string targetPath)
    {
        var sourceDir = Path.Combine(ApplicationBasePath, "RScripts");

        // If we have scripts in the app directory, copy them
        if (Directory.Exists(sourceDir))
            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(sourceDir.Length + 1);
                var targetFile = Path.Combine(targetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(file, targetFile, true);
            }
    }

    public string FindRInstallation()
    {
        // Check for environment variable first (allows users to override)
        // var rPathFromEnv = Environment.GetEnvironmentVariable("ACTIVESENSE_R_PATH");
        // if (!string.IsNullOrEmpty(rPathFromEnv) && File.Exists(rPathFromEnv)) return rPathFromEnv;
        //
        // if (OperatingSystem.IsWindows()) return FindWindowsRInstallation();
        //
        // if (OperatingSystem.IsMacOS()) return FindMacOSRInstallation();
        //
        // if (OperatingSystem.IsLinux()) return FindLinuxRInstallation();

        throw new FileNotFoundException(
            "Could not locate R installation on this system. Please install R or set the ACTIVESENSE_R_PATH environment variable.");
    }

    private string FindWindowsRInstallation()
    {
        // Check Program Files for R installations
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        // Common R installation paths on Windows
        var possiblePaths = new List<string>();

        // Check Program Files R directory
        var rDir = Path.Combine(programFiles, "R");
        if (Directory.Exists(rDir))
            // Look for R version directories like "R-4.2.2"
            foreach (var versionDir in Directory.GetDirectories(rDir))
            {
                var rscriptPath = Path.Combine(versionDir, "bin", "Rscript.exe");
                if (File.Exists(rscriptPath)) possiblePaths.Add(rscriptPath);
            }

        // Also check specific version installations (e.g., "R\R-4.4.3\bin\Rscript.exe")
        for (var version = 4.0; version <= 5.0; version += 0.1)
        {
            var versionString = version.ToString("F1", CultureInfo.InvariantCulture);
            var rscriptPath = Path.Combine(programFiles, "R", $"R-{versionString}", "bin", "Rscript.exe");
            if (File.Exists(rscriptPath)) possiblePaths.Add(rscriptPath);
        }

        // Check RStudio's R installation too
        var rstudioPath = Path.Combine(programFiles, "RStudio", "resources", "R", "bin", "Rscript.exe");
        if (File.Exists(rstudioPath)) possiblePaths.Add(rstudioPath);

        // Also check x86 Program Files
        var rstudioPathX86 = Path.Combine(programFilesX86, "RStudio", "resources", "R", "bin", "Rscript.exe");
        if (File.Exists(rstudioPathX86)) possiblePaths.Add(rstudioPathX86);

        if (possiblePaths.Count > 0)
            // Return the highest version (assuming the last in the list is newest)
            return possiblePaths[possiblePaths.Count - 1];

        throw new FileNotFoundException(
            "Could not locate R installation on Windows. Please install R from https://cran.r-project.org/bin/windows/base/");
    }

    private string FindMacOSRInstallation()
    {
        // Common R installation paths on macOS
        string[] possiblePaths =
        {
            "/usr/local/bin/Rscript",
            "/usr/bin/Rscript",
            "/opt/R/bin/Rscript",
            "/Library/Frameworks/R.framework/Resources/bin/Rscript"
        };

        foreach (var path in possiblePaths)
            if (File.Exists(path))
                return path;

        // Try to find using 'which' command
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/which",
                Arguments = "Rscript",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output) && File.Exists(output)) return output;
        }
        catch
        {
            // Ignore errors and continue with other methods
        }

        throw new FileNotFoundException(
            "Could not locate R installation on macOS. Please install R from https://cran.r-project.org/bin/macosx/");
    }

    private static string FindLinuxRInstallation()
    {
        // Common R installation paths on Linux
        string[] possiblePaths =
        {
            "/usr/bin/Rscript",
            "/usr/local/bin/Rscript",
            "/opt/R/bin/Rscript"
        };

        foreach (var path in possiblePaths)
            if (File.Exists(path))
                return path;

        // Try to find using 'which' command
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "Rscript",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output) && File.Exists(output)) return output;
        }
        catch
        {
            // Ignore errors and continue with other methods
        }

        throw new FileNotFoundException(
            "Could not locate R installation on Linux. Please install R using your package manager (e.g., 'sudo apt install r-base' on Debian/Ubuntu)");
    }
}