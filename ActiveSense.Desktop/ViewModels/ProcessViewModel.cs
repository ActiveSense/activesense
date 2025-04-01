using System;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Helpers;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessViewModel : ViewModelBase
{
    public Interaction<string, string[]?> SelectFilesInteraction { get; } = new Interaction<string, string[]?>();
    private readonly IRScriptService _rsScriptService;
    private readonly IFileService _fileService;

    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private string _statusMessage = "No files selected";
    [ObservableProperty] private bool _isProcessing;

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
            var rDataPath = _rsScriptService.GetRDataPath();
            var success = await _fileService.CopyFilesToDirectoryAsync(SelectedFiles, rDataPath);
            
            if (!success)
            {
                StatusMessage = "Failed to copy files";
                return;
            }
            
            var rScriptPath = Path.Combine(_rsScriptService.GetRScriptBasePath(), "_main.R");
            
            var (scriptSuccess, output, error) = await _rsScriptService.ExecuteScriptAsync(
                rScriptPath, _rsScriptService.GetRScriptBasePath(), "");
            
            if (!scriptSuccess)
            {
                StatusMessage = $"Script execution failed: {error}";
                return;
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}