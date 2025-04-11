using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop;
using ActiveSense.Desktop.Helpers;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessViewModel : ViewModelBase
{
    private readonly IFileService _fileService;
    private readonly IResultParserService _resultParserService;
    private readonly IRScriptService _rScriptService;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _scriptOutput = string.Empty;

    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private bool _showScriptOutput;
    [ObservableProperty] private string _statusMessage = "No files selected";

    public ProcessViewModel()
    {
        _rScriptService = new RScriptService("Rscript");
        _fileService = new FileService();
        _resultParserService = new ResultParserService();
    }

    public Interaction<string, string[]?> SelectFilesInteraction { get; } = new();

    [RelayCommand]
    private async Task SelectFilesAsync()
    {
        SelectedFiles = await SelectFilesInteraction.HandleAsync("Hello test");
    }

    [RelayCommand]
    private async Task ProcessFiles()
    {
        if (SelectedFiles is null || SelectedFiles.Length == 0)
        {
            StatusMessage = "No files selected";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Processing files...";

        try
        {
            var processingDirectory = _rScriptService.GetRDataPath();
            var outputsDirectory = _rScriptService.GetROutputPath();
            var success = await _fileService.CopyFilesToDirectoryAsync(SelectedFiles, processingDirectory, outputsDirectory);

            if (!success)
            {
                StatusMessage = "Failed to copy files";
                return;
            }

            var rScriptPath = Path.Combine(_rScriptService.GetRScriptBasePath(), "_main.R");

            var (scriptSuccess, output, error) = await _rScriptService.ExecuteScriptAsync(
                rScriptPath, _rScriptService.GetRScriptBasePath(), $"-d {AppConfig.SolutionBasePath}");

            ScriptOutput = output;
            ShowScriptOutput = true;

            if (!scriptSuccess)
            {
                StatusMessage = $"Script execution failed: {error}";
                return;
            }

            StatusMessage = "Success";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}