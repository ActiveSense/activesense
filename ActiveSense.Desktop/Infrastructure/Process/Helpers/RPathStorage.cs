using System;
using System.IO;

namespace ActiveSense.Desktop.Infrastructure.Process.Helpers;

public static class RPathStorage
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ActiveSense", "rpath.txt");
    
    public static string GetRPath()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return File.ReadAllText(FilePath).Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading R path: {ex.Message}");
        }
        return string.Empty;
    }
    
    public static void SaveRPath(string path)
    {
        try
        {
            string? directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(FilePath, path);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving R path: {ex.Message}", ex);
        }
    }
    public static bool TestRExecutable(string path)
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Rscript");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing R executable: {ex.Message}");
            return false;
        }
    }
}