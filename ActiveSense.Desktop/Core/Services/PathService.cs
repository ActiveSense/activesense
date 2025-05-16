using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;

namespace ActiveSense.Desktop.Core.Services;

public class PathService : IPathService
{
    private readonly string _customOutputPath;
    private readonly string _customScriptPath;

    public PathService(string customOutputPath = null, string customScriptPath = null)
    {
        _customOutputPath = customOutputPath;
        _customScriptPath = customScriptPath;

        // directories exist
        EnsureDirectoryExists(OutputDirectory);
        EnsureDirectoryExists(ScriptInputPath);
    }

    public string ApplicationBasePath => AppDomain.CurrentDomain.BaseDirectory;


    public string BasePath
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

    public string ScriptInputPath => CombinePaths(ScriptBasePath, "data");
    public string MainScriptPath => CombinePaths(ScriptBasePath, "_main.R");
    public string ScriptExecutablePath => FindRInstallation();

    public string OutputDirectory
    {
        get
        {
            // If custom output path is provided, use it
            if (!string.IsNullOrEmpty(_customOutputPath))
                return _customOutputPath;

            // Check if we're in development mode or production
            if (IsInDevelopmentMode())
                // In development, use the solution directory
                return CombinePaths(BasePath, "AnalysisFiles/");

            // In production, use the user's local app data folder
            var userOutputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ActiveSense", "AnalysisFiles/");

            // Ensure the directory exists
            EnsureDirectoryExists(userOutputPath);
            return userOutputPath;
        }
    }

    public string ScriptBasePath
    {
        get
        {
            // Check relative to application first
            var relativePath = CombinePaths(BasePath, "../ActiveSense.RScripts");
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

// Helper function to determine if we're in development mode
    private bool IsInDevelopmentMode()
    {
        // If we're in the solution directory structure (containing .git, .idea, etc.)
        // or if BasePath contains "bin/Debug", we're likely in development mode
        return Directory.Exists(Path.Combine(BasePath, ".git")) ||
               Directory.Exists(Path.Combine(BasePath, ".idea")) ||
               BasePath.Contains("bin\\Debug") ||
               File.Exists(Path.Combine(BasePath, "ActiveSense.Desktop.sln"));
    }

    #region helpers

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
        {
            // First ensure the target directory exists
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            // Copy all files and subdirectories
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                // Create the corresponding subdirectory in the target location
                var newDirPath = dirPath.Replace(sourceDir, targetPath);
                if (!Directory.Exists(newDirPath)) Directory.CreateDirectory(newDirPath);
            }

            // Copy all files
            foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                var newFilePath = filePath.Replace(sourceDir, targetPath);
                File.Copy(filePath, newFilePath, true);
            }
        }
    }

    public string FindRInstallation()
    {
        var savedPath = RPathStorage.GetRPath();
        if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath) && RPathStorage.TestRExecutable(savedPath))
            return savedPath;


        if (OperatingSystem.IsWindows()) savedPath = FindWindowsRInstallation();

        if (OperatingSystem.IsMacOS()) savedPath = FindMacOSRInstallation();

        if (OperatingSystem.IsLinux()) savedPath = FindLinuxRInstallation();

        if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath) && RPathStorage.TestRExecutable(savedPath))
            return savedPath;

        throw new FileNotFoundException(
            "Could not locate R installation on this system. Please install R or set the path manually.");
    }

    private string FindWindowsRInstallation()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        var possiblePaths = new List<string>();

        var rDir = Path.Combine(programFiles, "R");
        if (Directory.Exists(rDir))
            foreach (var versionDir in Directory.GetDirectories(rDir))
            {
                var rscriptPath = Path.Combine(versionDir, "bin", "Rscript.exe");
                if (File.Exists(rscriptPath)) possiblePaths.Add(rscriptPath);
            }

        for (var version = 4.0; version <= 5.0; version += 0.1)
        {
            var versionString = version.ToString("F1", CultureInfo.InvariantCulture);
            var rscriptPath = Path.Combine(programFiles, "R", $"R-{versionString}", "bin", "Rscript.exe");
            if (File.Exists(rscriptPath)) possiblePaths.Add(rscriptPath);
        }

        var rstudioPath = Path.Combine(programFiles, "RStudio", "resources", "R", "bin", "Rscript.exe");
        if (File.Exists(rstudioPath)) possiblePaths.Add(rstudioPath);

        var rstudioPathX86 = Path.Combine(programFilesX86, "RStudio", "resources", "R", "bin", "Rscript.exe");
        if (File.Exists(rstudioPathX86)) possiblePaths.Add(rstudioPathX86);

        if (possiblePaths.Count > 0)
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
        string[] possiblePaths =
        {
            "/usr/bin/Rscript",
            "/usr/local/bin/Rscript",
            "/opt/R/bin/Rscript"
        };

        foreach (var path in possiblePaths)
            if (File.Exists(path) && RPathStorage.TestRExecutable(path))
                return path;

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
        }

        throw new FileNotFoundException(
            "Could not locate R installation on Linux. Please install R using your package manager (e.g., 'sudo apt install r-base' on Debian/Ubuntu)");
    }

    #endregion
}