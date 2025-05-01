using System;
using System.IO;

namespace ActiveSense.Desktop;

public static class AppConfig
{
    private static string _solutionBasePath = string.Empty;
    
    /// <summary>
    /// Gets the base directory of the solution
    /// </summary>
    public static string SolutionBasePath
    {
        get
        {
            if (string.IsNullOrEmpty(_solutionBasePath))
            {
                _solutionBasePath = CalculateSolutionBasePath();
            }
            
            return _solutionBasePath;
        }
    }
    
    /// <summary>
    /// Gets the outputs directory path
    /// </summary>
    public static string OutputsDirectoryPath => Path.Combine(SolutionBasePath, "AnalysisFiles/");
    
    private static string CalculateSolutionBasePath()
    {
        string directory = AppDomain.CurrentDomain.BaseDirectory;
        
        if (directory.Contains("bin"))
        {
            while (!Directory.Exists(Path.Combine(directory, "ActiveSense.Desktop")) && 
                   !File.Exists(Path.Combine(directory, "ActiveSense.Desktop.sln")))
            {
                DirectoryInfo? parentDir = Directory.GetParent(directory);
                if (parentDir == null)
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
                
                directory = parentDir.FullName;
            }
        }
        
        string outputsPath = Path.Combine(directory, "outputs");
        if (!Directory.Exists(outputsPath))
        {
            Directory.CreateDirectory(outputsPath);
        }
        
        return directory;
    }
}