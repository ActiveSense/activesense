using System;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessDialogViewModel(
    SensorProcessorFactory sensorProcessorFactory,
    IScriptService scriptService) : DialogViewModel
{
    [ObservableProperty] private string _cancelText = "No";

    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _confirmText = "Yes";
    [ObservableProperty] private string _iconText = "\xe4e0";

    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _message = "Are you sure?";
    [ObservableProperty] private string _scriptOutput = string.Empty;
    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private SensorTypes _selectedSensorTypes = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showScriptOutput;
    [ObservableProperty] private string _statusMessage = "No files selected";
    [ObservableProperty] private string _test = "TestText";
    [ObservableProperty] private string _title = "Confirm";

    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }

    public event Action<string[]?>? FilesSelected;

    public void SetSelectedFiles(string[]? files)
    {
        SelectedFiles = files;
        FilesSelected?.Invoke(files);
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