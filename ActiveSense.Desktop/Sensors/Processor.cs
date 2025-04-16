using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;

namespace ActiveSense.Desktop.Sensors
{
    public class GeneActivProcessor : ISensorProcessor
    {
        private readonly IScriptService _rScriptService;

        public SensorTypes SupportedType => SensorTypes.GENEActiv;

        public static string[] SupportedFileTypes => new[] { ".csv", ".bin" };

        public GeneActivProcessor(IScriptService rScriptService)
        {
            _rScriptService = new RScriptService();
        }

        public async Task<(bool Success, string Output, string Error)> ProcessAsync(string arguments = "")
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _rScriptService.GetExecutablePath(),
                        Arguments = $"\"{_rScriptService.GetScriptPath()}\" {arguments}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = _rScriptService.GetScriptBasePath()
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
            catch (Exception ex)
            {
                return (false, string.Empty, $"Failed to execute R script: {ex.Message}");
            }
        }

        // private async Task<bool> MoveFilesForProcessing(IEnumerable<string> sourcePaths,
        //     string destinationProcessingDirectory, string destinationCompleteAnalyses)
        // {
        //     try
        //     {
        //         Directory.CreateDirectory(destinationCompleteAnalyses);
        //         Directory.CreateDirectory(destinationProcessingDirectory);
        //
        //         foreach (var sourcePath in sourcePaths)
        //         {
        //             var fileExtension = Path.GetExtension(sourcePath);
        //             var fileName = Path.GetFileName(sourcePath);
        //
        //             string? destinationPath = fileExtension switch
        //             {
        //                 ".csv" => Path.Combine(destinationCompleteAnalyses, fileName),
        //                 ".bin" => Path.Combine(destinationProcessingDirectory, fileName),
        //                 _ => null
        //             };
        //
        //             if (destinationPath != null)
        //             {
        //                 if (File.Exists(destinationPath))
        //                 {
        //                     File.Delete(destinationPath);
        //                 }
        //
        //                 await Task.Run(() => File.Copy(sourcePath, destinationPath));
        //             }
        //         }
        //
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error moving files: {ex.Message}");
        //         return false;
        //     }
        // }
    }
}