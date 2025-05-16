namespace ActiveSense.Desktop.Core.Services.Interfaces;

public interface IPathService
{
    // Base paths
    string ApplicationBasePath { get; }
    string BasePath { get; }
    
    // Application directories
    string OutputDirectory { get; }
    
    // Script paths and functions
    string ScriptBasePath { get; }
    string ScriptInputPath { get; }
    string MainScriptPath { get; }
    string ScriptExecutablePath { get; } // Formerly GetExecutablePath()
    
    // Utility functions
    string CombinePaths(params string[] paths);
    bool EnsureDirectoryExists(string path);
    void ClearDirectory(string path);
}