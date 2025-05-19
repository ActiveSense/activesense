namespace ActiveSense.Desktop.Core.Services.Interfaces;

public interface IPathService
{
    // Application directories
    string OutputDirectory { get; }
    
    // Script paths and functions
    string ScriptBasePath { get; }
    string ScriptInputPath { get; }
    string MainScriptPath { get; }
    string ScriptExecutablePath { get; } 
    
    // Utility functions
    bool EnsureDirectoryExists(string path);
    void ClearDirectory(string path);
    void CopyResources();
}