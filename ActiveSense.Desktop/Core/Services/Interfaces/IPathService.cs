namespace ActiveSense.Desktop.Core.Services.Interfaces;

public interface IPathService
{
    string OutputDirectory { get; }

    string ScriptBasePath { get; }
    string ScriptInputPath { get; }
    string MainScriptPath { get; }
    string ScriptExecutablePath { get; }

    bool EnsureDirectoryExists(string path);
    void ClearDirectory(string path);
    void CopyResources();
}