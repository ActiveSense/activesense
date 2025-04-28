using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.IO;

namespace ActiveSense.Desktop.ViewModels;

public partial class ExportDialogViewModel : DialogViewModel
{
    private readonly ExporterFactory _exporterFactory;
    private readonly SharedDataService _sharedDataService;
    
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _outputPath = string.Empty;
    [ObservableProperty] private string _statusMessage = "Select a folder to export analyses";
    [ObservableProperty] private bool _isExporting = false;
    [ObservableProperty] private bool _isExportSuccessful = false;
    [ObservableProperty] private SensorTypes _selectedSensorType = SensorTypes.GENEActiv;
    
    public ExportDialogViewModel(
        ExporterFactory exporterFactory,
        SharedDataService sharedDataService)
    {
        _exporterFactory = exporterFactory;
        _sharedDataService = sharedDataService;
    }
    
    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }
    
    [RelayCommand]
    public async Task ExportAnalyses()
    {
        if (string.IsNullOrEmpty(OutputPath))
        {
            StatusMessage = "Please select a folder first";
            return;
        }
        
        if (_sharedDataService.SelectedAnalyses.Count == 0)
        {
            StatusMessage = "No analyses selected for export";
            return;
        }
        
        IsExporting = true;
        StatusMessage = "Exporting analyses...";
        
        try
        {
            var exporter = _exporterFactory.GetExporter(_selectedSensorType);
            var totalExports = 0;
            var exportedAnalyses = new List<Analysis>();
            
            foreach (var analysis in _sharedDataService.SelectedAnalyses)
            {
                var pdfPath = System.IO.Path.Combine(OutputPath, $"{analysis.FileName}.pdf");
                var success = await exporter.ExportAsync(analysis, pdfPath);
                
                if (success)
                {
                    totalExports++;
                    analysis.Exported = true;
                    exportedAnalyses.Add(analysis);
                }
            }
            
            if (totalExports > 0)
            {
                _sharedDataService.UpdateAllAnalyses(exportedAnalyses);
                StatusMessage = $"Successfully exported {totalExports} analyses to {OutputPath}";
                IsExportSuccessful = true;
            }
            else
            {
                StatusMessage = "Failed to export any analyses";
                IsExportSuccessful = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during export: {ex.Message}";
            IsExportSuccessful = false;
        }
        finally
        {
            IsExporting = false;
        }
    }
}