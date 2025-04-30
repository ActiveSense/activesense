using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using iText.Layout.Element;

namespace ActiveSense.Desktop.Sensors
{
    public class GeneActivProcessor : ISensorProcessor
    {
        private readonly IScriptService _rScriptService;
        private readonly List<ScriptArgument> _defaultArguments;

        public GeneActivProcessor(IScriptService rScriptService)
        {
            _rScriptService = rScriptService ?? new RScriptService();
            _defaultArguments = CreateDefaultArguments();
        }

        public SensorTypes SupportedType => SensorTypes.GENEActiv;
        public static string[] SupportedFileTypes => new[] { ".csv", ".bin" };
        public IReadOnlyList<ScriptArgument> DefaultArguments => _defaultArguments;

        private List<ScriptArgument> CreateDefaultArguments()
        {
            return new List<ScriptArgument>
            {
                // Boolean arguments
                new BoolArgument
                {
                    Flag = "a",
                    Name = "Activity Analysis",
                    Description = "Run activity analysis",
                    Value = true
                },
                new BoolArgument
                {
                    Flag = "s",
                    Name = "Sleep Analysis",
                    Description = "Run sleep analysis",
                    Value = true
                },
            };
        }

       public async Task<(bool Success, string Output, string Error)> ProcessAsync(
            IEnumerable<ScriptArgument> arguments = null)
        {
            try
            {
                var scriptPath = _rScriptService.GetScriptPath();
                var executablePath = _rScriptService.GetExecutablePath();
                var workingDirectory = _rScriptService.GetScriptBasePath();
                
                var argsToUse = arguments?.ToList() ?? _defaultArguments;
                
                var outputDir = $"-d \"{AppConfig.OutputsDirectoryPath}\"";
                
                var scriptArguments = string.Join(" ", 
                    argsToUse
                        .Select(arg => arg.ToCommandLineArgument())
                        .Where(arg => !string.IsNullOrEmpty(arg)));
                
                var processArguments = $"\"{scriptPath}\" {outputDir} {scriptArguments}";
                
                return await ExecuteProcessAsync(executablePath, processArguments, workingDirectory);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"Failed to execute R script: {ex.Message}");
            }
        }

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

        public void ClearProcessingDirectory(string processingDirectory)
        {
            if (Directory.Exists(processingDirectory))
            {
                try
                {
                    Directory.Delete(processingDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing processing directory: {ex.Message}");
                }
            }
        }
        public void CopyFiles(string[] files, string processingDirectory, string outputDirectory)
        {
            if (files == null || files.Length == 0)
            {
                return;
            }
            
            ClearProcessingDirectory(processingDirectory);
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