using System;
using System.IO;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Services;

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
            string directory = ApplicationBasePath;

            if (directory.Contains("bin"))
            {
                while (!Directory.Exists(Path.Combine(directory, "ActiveSense.Desktop")) &&
                       !File.Exists(Path.Combine(directory, "ActiveSense.Desktop.sln")))
                {
                    DirectoryInfo? parentDir = Directory.GetParent(directory);
                    if (parentDir == null)
                    {
                        return ApplicationBasePath;
                    }

                    directory = parentDir.FullName;
                }
            }
            
            return directory;
        }
    }
    
    public string OutputDirectory => CombinePaths(SolutionBasePath, "AnalysisFiles/");
    
    // Script paths
    public string ScriptInputPath => CombinePaths(ScriptBasePath, "data");
    public string MainScriptPath => CombinePaths(ScriptBasePath, "_main.R");
    public string ScriptExecutablePath => "Rscript";
    
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
        {
            try
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing directory: {ex.Message}");
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }
    private void CopyResourceScripts(string targetPath)
    {
        var sourceDir = Path.Combine(ApplicationBasePath, "RScripts");
        
        // If we have scripts in the app directory, copy them
        if (Directory.Exists(sourceDir))
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(sourceDir.Length + 1);
                var targetFile = Path.Combine(targetPath, relativePath);
                
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(file, targetFile, true);
            }
        }
    }
}