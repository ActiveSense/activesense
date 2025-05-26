using System;
using System.Diagnostics;
using System.IO;

namespace ActiveSense.Desktop.Infrastructure.Process.Helpers;

public static class RPathStorage
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ActiveSense", "rpath.txt");

    public static string GetRPath()
    {
        return File.Exists(FilePath) ? File.ReadAllText(FilePath).Trim() : string.Empty;
    }

    public static void SaveRPath(string path)
    {
        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory) && directory != null) Directory.CreateDirectory(directory);
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
            process.StartInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Rscript");
        }
        catch (Exception)
        {
            return false;
        }
    }
}