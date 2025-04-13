using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Helpers;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessPageViewModel(
    SensorProcessorFactory sensorProcessorFactory,
    IScriptService scriptService,
    MainViewModel mainViewModel,
    DialogService dialogService)
    : PageViewModel
{
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _scriptOutput = string.Empty;
    [ObservableProperty] private SensorTypes _selectedSensorTypes = SensorTypes.GENEActiv;
    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private bool _showScriptOutput;
    [ObservableProperty] private string _statusMessage = "No files selected";

    public Interaction<string, string[]?> SelectFilesInteraction { get; } = new();

    [RelayCommand]
    private async Task SelectFilesAsync()
    {
        SelectedFiles = await SelectFilesInteraction.HandleAsync("Select files to process");
        if (SelectedFiles != null && SelectedFiles.Length > 0)
        {
            StatusMessage = $"{SelectedFiles.Length} file(s) selected";
        }
        else
        {
            StatusMessage = "No files selected";
        }
    }

    [RelayCommand]
    public async Task TriggerDialog()
    {
        var dialog = new ProcessDialogViewModel
        {
            Title = "Confirm",
            Message = "Are you sure you want to process the selected files?",
        };
        
        await dialogService.ShowDialog<MainViewModel, ProcessDialogViewModel>(mainViewModel, dialog);
        
        if (!dialog.Confirmed)
        {
            Console.WriteLine("Dialog was cancelled");
        }
    }

    [RelayCommand]
    private async Task ProcessFiles()
    {
        
        if (SelectedFiles is null || SelectedFiles.Length == 0)
        {
            StatusMessage = "No files selected";
            return;
        }

        var processor = sensorProcessorFactory.GetSensorProcessor(SelectedSensorTypes);
        
        IsProcessing = true;
        StatusMessage = "Processing files...";

        try
        {
            var destinationDirectory = scriptService.GetScriptInputPath();
            var success = await FileService.CopyFilesToDirectoryAsync(SelectedFiles, destinationDirectory);

            if (!success)
            {
                StatusMessage = "Failed to copy files";
                return;
            }

            var (scriptSuccess, output, error) = await processor.ProcessAsync($"-d {AppConfig.OutputsDirectoryPath}");

            ScriptOutput = output;
            ShowScriptOutput = true;

            if (!scriptSuccess)
            {
                StatusMessage = $"Script execution failed: {error}";
                return;
            }

            StatusMessage = "Processing completed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ScriptOutput = ex.ToString();
            ShowScriptOutput = true;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}