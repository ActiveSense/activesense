using System;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ExportDialogViewModel(
    ExporterFactory exporterFactory,
    SharedDataService sharedDataService,
    MainViewModel mainViewModel,
    DialogService dialogService)
    : DialogViewModel
{
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _statusMessage = "Choose export options";
    [ObservableProperty] private SensorTypes _selectedSensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _includeRawData = false;
    
    public event Func<bool, Task<string?>>? FilePickerRequested;

    public int SelectedAnalysesCount => sharedDataService.SelectedAnalyses.Count;
    
    public IAnalysis GetFirstSelectedAnalysis()
    {
        return sharedDataService.SelectedAnalyses.FirstOrDefault();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Confirmed = false;
        Close();
    }
    
    [RelayCommand]
    private async Task ExportAnalysis()
    {
        if (sharedDataService.SelectedAnalyses.Count != 1)
        {
            StatusMessage = "Please select exactly one analysis to export";
            return;
        }
        
        var filePath = await FilePickerRequested?.Invoke(IncludeRawData)!;
        
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        
        try
        {
            var exporter = exporterFactory.GetExporter(SelectedSensorType);
            var analysis = sharedDataService.SelectedAnalyses.First();
            
            var success = await exporter.ExportAsync(analysis, filePath, IncludeRawData);
            
            if (success)
            {
                analysis.Exported = true;
                sharedDataService.UpdateAllAnalyses([analysis]);
            }
            else
            {
                var dialog = new WarningDialogViewModel()
                {
                    Title = "Export fehlgeschlagen",
                    Message = "Export failed. Please check the file path and try again.",
                    OkButtonText = "OK",
                    CloseButtonText = "Abbrechen"
                };
                await dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(mainViewModel, dialog);
            }

            Close();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during export: {ex.Message}";
        }
    }
}