using System;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ExportDialogViewModel(
    ExporterFactory exporterFactory,
    ISharedDataService sharedDataService,
    MainViewModel mainViewModel,
    DialogService dialogService)
    : DialogViewModel
{
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private bool _exportStarted;
    [ObservableProperty] private bool _includeRawData;
    [ObservableProperty] private SensorTypes _selectedSensorType = SensorTypes.GENEActiv;

    public int SelectedAnalysesCount => sharedDataService.SelectedAnalyses.Count;

    public event Func<bool, Task<string?>>? FilePickerRequested;

    public IAnalysis GetFirstSelectedAnalysis()
    {
        return sharedDataService.SelectedAnalyses.First();
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
        var filePath = await FilePickerRequested?.Invoke(IncludeRawData)!;

        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            var exporter = exporterFactory.GetExporter(SelectedSensorType);
            var analysis = sharedDataService.SelectedAnalyses.First();

            ExportStarted = true;
            var success = await exporter.ExportAsync(analysis, filePath, IncludeRawData);

            if (success)
            {
                analysis.Exported = true;
                sharedDataService.UpdateAllAnalyses([analysis]);
            }
            else
            {
                var dialog = new InfoDialogViewModel
                {
                    Title = "Export fehlgeschlagen",
                    Message = "Export failed. Please check the file path and try again.",
                    OkButtonText = "Schliessen"
                };
                await dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(mainViewModel, dialog);
            }

            Close();
        }
        catch (Exception ex)
        {
            var dialog = new InfoDialogViewModel
            {
                Title = "Export fehlgeschlagen",
                Message = "Fehler beim Exportieren",
                ExtendedMessage = ex.Message,
                OkButtonText = "Schliessen"
            };
            await dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(mainViewModel, dialog);
        }
        finally
        {
            ExportStarted = false;
        }
    }
}