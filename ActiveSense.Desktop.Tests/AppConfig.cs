using System;
using System.IO;

namespace ActiveSense.Desktop.Tests;

public static class AppConfig
{
    private static string _solutionBasePath;

    /// <summary>
    ///     Gets the base directory of the solution
    /// </summary>
    public static string SolutionBasePath
    {
        get
        {
            if (string.IsNullOrEmpty(_solutionBasePath)) _solutionBasePath = CalculateSolutionBasePath();

            return _solutionBasePath;
        }
    }

    /// <summary>
    ///     Gets the outputs directory path
    /// </summary>
    public static string OutputsDirectoryPath =>
        Path.Combine(SolutionBasePath, "ActiveSense.Desktop.Tests/Tests/AnalysisTestFiles/outputs");

    public static string InputDirectoryPath =>
        Path.Combine(SolutionBasePath, "ActiveSense.Desktop.Tests/Tests/AnalysisTestFiles/input");

    public static string DiffsDirectoryPath =>
        Path.Combine(SolutionBasePath, "ActiveSense.Desktop.Tests/Tests/AnalysisTestFiles/diffs");

    private static string CalculateSolutionBasePath()
    {
        // Start with the executable directory
        var directory = AppDomain.CurrentDomain.BaseDirectory;

        // For development environment
        if (directory.Contains("bin"))
            // Go up until we find the solution directory
            while (!Directory.Exists(Path.Combine(directory, "ActiveSense.Desktop")) &&
                   !File.Exists(Path.Combine(directory, "ActiveSense.Desktop.sln")))
            {
                var parentDir = Directory.GetParent(directory);
                if (parentDir == null)
                    // If we can't find it, fall back to the executable directory
                    return AppDomain.CurrentDomain.BaseDirectory;

                directory = parentDir.FullName;
            }

        // Ensure the outputs directory exists
        var outputsPath = Path.Combine(directory, "outputs");
        if (!Directory.Exists(outputsPath)) Directory.CreateDirectory(outputsPath);

        return directory;
    }
}