using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;

namespace ActiveSense.Desktop.Sensors
{
    public class GeneActivProcessor(IScriptService rScriptService) : ISensorProcessor
    {
        private readonly IScriptService _rScriptService = rScriptService ?? new RScriptService();

        public SensorTypes SupportedType => SensorTypes.GENEActiv;

        public static string[] SupportedFileTypes => new[] { ".csv", ".bin" };

        public async Task<(bool Success, string Output, string Error)> ProcessAsync(string arguments = "")
        {
            try
            {
                var scriptPath = _rScriptService.GetScriptPath();
                var executablePath = _rScriptService.GetExecutablePath();
                var workingDirectory = _rScriptService.GetScriptBasePath();
                
                var processArguments = $"\"{scriptPath}\" {arguments}";
                return await ExecuteProcessAsync(executablePath, processArguments, workingDirectory);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"Failed to execute R script: {ex.Message}");
            }
        }
        
        // Protected virtual method that can be overridden in tests
        protected virtual async Task<(bool Success, string Output, string Error)> ExecuteProcessAsync(
            string scriptPath, string arguments, string workingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = scriptPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            string output = await outputTask;
            string error = await errorTask;

            return (process.ExitCode == 0, output, error);
        }
        
        public void CopyFiles(string[] files, string processingDirectory, string outputDirectory)
        {
            if (files == null || files.Length == 0)
            {
                return;
            }

            Directory.CreateDirectory(processingDirectory);
            Directory.CreateDirectory(outputDirectory);

            foreach (var file in files)
            {
                try
                {
                    string extension = Path.GetExtension(file).ToLowerInvariant();
                    string fileName = Path.GetFileName(file);
            
                    if (extension == ".bin")
                    {
                        string destinationPath = Path.Combine(processingDirectory, fileName);
                        File.Copy(file, destinationPath, overwrite: true);
                    }
                    else if (extension == ".pdf")
                    {
                        string destinationPath = Path.Combine(outputDirectory, fileName);
                        File.Copy(file, destinationPath, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {file}: {ex.Message}");
                }
            }
        }
    }
}