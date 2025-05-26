using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using Serilog;

namespace ActiveSense.Desktop.Core.Services;

public class PathService : IPathService
{
    private readonly ILogger _logger;

    public PathService(ILogger logger)
    {
        _logger = logger;
        EnsureDirectoryExists(OutputDirectory);
        EnsureDirectoryExists(ScriptInputPath);
    }

    public string ApplicationBasePath => AppDomain.CurrentDomain.BaseDirectory;

    private string SolutionBasePath
    {
        get
        {
            var dir = ApplicationBasePath;
            while (!Directory.Exists(Path.Combine(dir, "ActiveSense.Desktop")) &&
                   !File.Exists(Path.Combine(dir, "ActiveSense.Desktop.sln")))
            {
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }

            return dir;
        }
    }

    private bool IsDevelopment =>
        Directory.Exists(Path.Combine(SolutionBasePath, ".git")) ||
        Directory.Exists(Path.Combine(SolutionBasePath, ".idea")) ||
        SolutionBasePath.Contains("bin\\Debug") ||
        File.Exists(Path.Combine(SolutionBasePath, "ActiveSense.Desktop.sln"));

    public string OutputDirectory => IsDevelopment
        ? CombinePaths(SolutionBasePath, "AnalysisFiles/")
        : GetOrCreateLocalAppPath("AnalysisFiles/");

    public string ScriptBasePath
    {
        get
        {
            var devPath = CombinePaths(SolutionBasePath, "../ActiveSense.RScripts");
            if (Directory.Exists(devPath)) return devPath;

            var userPath = GetOrCreateLocalAppPath("RScripts");

            return userPath;
        }
    }

    public string ScriptInputPath => CombinePaths(ScriptBasePath, "data");
    public string MainScriptPath => CombinePaths(ScriptBasePath, "_main.R");
    public string ScriptExecutablePath => FindRInstallation();

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
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Could not clear directory");
        }
    }

    public void CopyResources()
    {
        var sourceDir = Path.Combine(ApplicationBasePath, "RScripts");
        var targetPath = GetOrCreateLocalAppPath("RScripts");

        _logger.Information("Copying resources from {source} to {target}", sourceDir, targetPath);

        foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            EnsureDirectoryExists(dirPath.Replace(sourceDir, targetPath));

        foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            File.Copy(filePath, filePath.Replace(sourceDir, targetPath), true);
    }

    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    private string GetOrCreateLocalAppPath(string folder)
    {
        return EnsureAndReturn(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ActiveSense", folder));
    }

    private string EnsureAndReturn(string path)
    {
        EnsureDirectoryExists(path);
        return path;
    }

    private string FindRInstallation()
    {
        var saved = RPathStorage.GetRPath();
        if (IsValidRPath(saved)) return saved;

        var platformPath = OperatingSystem.IsWindows() ? FindWindowsRInstallation()
            : OperatingSystem.IsMacOS() ? FindMacOSRInstallation()
            : OperatingSystem.IsLinux() ? FindLinuxRInstallation()
            : null;

        if (IsValidRPath(platformPath)) return platformPath;

        throw new FileNotFoundException("Could not locate R installation.");
    }

    private bool IsValidRPath(string path)
    {
        return !string.IsNullOrEmpty(path) && File.Exists(path) && RPathStorage.TestRExecutable(path);
    }

    private string FindWindowsRInstallation()
    {
        var paths = new List<string>();
        string[] baseDirs =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        foreach (var baseDir in baseDirs)
        {
            var rBase = Path.Combine(baseDir, "R");
            if (Directory.Exists(rBase))
                foreach (var ver in Directory.GetDirectories(rBase))
                    paths.Add(Path.Combine(ver, "bin", "Rscript.exe"));

            for (var v = 4.0; v <= 5.0; v += 0.1)
            {
                var version = $"R-{v:F1}";
                paths.Add(Path.Combine(baseDir, "R", version, "bin", "Rscript.exe"));
            }

            paths.Add(Path.Combine(baseDir, "RStudio", "resources", "R", "bin", "Rscript.exe"));
        }

        return paths.Find(File.Exists) ?? throw new FileNotFoundException("No R installation found on Windows.");
    }

    private string FindMacOSRInstallation()
    {
        var paths = new[]
        {
            "/usr/local/bin/Rscript",
            "/usr/bin/Rscript",
            "/opt/R/bin/Rscript",
            "/Library/Frameworks/R.framework/Resources/bin/Rscript"
        };

        foreach (var path in paths)
            if (File.Exists(path))
                return path;

        return RunWhich("Rscript") ?? throw new FileNotFoundException("No R installation found on macOS.");
    }

    private static string FindLinuxRInstallation()
    {
        var paths = new[]
        {
            "/usr/bin/Rscript",
            "/usr/local/bin/Rscript",
            "/opt/R/bin/Rscript"
        };

        foreach (var path in paths)
            if (File.Exists(path) && RPathStorage.TestRExecutable(path))
                return path;

        return RunWhich("Rscript") ?? throw new FileNotFoundException("No R installation found on Linux.");
    }

    private static string? RunWhich(string command)
    {
        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "where" : "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            return File.Exists(output) ? output : null;
        }
        catch
        {
            return null;
        }
    }
}