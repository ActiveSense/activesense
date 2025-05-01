using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessDialogViewModel : DialogViewModel
{
    private readonly SensorProcessorFactory _sensorProcessorFactory;
    private readonly IScriptService _scriptService;
    private readonly SharedDataService _sharedDataService;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly DialogService _dialogService;
    private readonly MainViewModel _mainViewModel;
    
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private string _cancelText = "Cancel";
    [ObservableProperty] private string _confirmText = "Confirm";
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _scriptOutput = string.Empty;
    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private SensorTypes _selectedSensorTypes = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showScriptOutput;
    [ObservableProperty] private string _statusMessage = "No files selected";
    [ObservableProperty] private string _title = "Sensordaten analysieren";
    
    [ObservableProperty] private ObservableCollection<ScriptArgument> _arguments = new();

    public ProcessDialogViewModel(SensorProcessorFactory sensorProcessorFactory, IScriptService scriptService, SharedDataService sharedDataService, ResultParserFactory resultParserFactory, DialogService dialogService, MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _dialogService = dialogService;
        _resultParserFactory = resultParserFactory;
        _sensorProcessorFactory = sensorProcessorFactory;
        _scriptService = scriptService;
        _sharedDataService = sharedDataService;
        
        LoadDefaultArguments();
    }

    private void LoadDefaultArguments()
    {
        var processor = _sensorProcessorFactory.GetSensorProcessor(SelectedSensorTypes);
        Arguments.Clear();
        
        foreach (var arg in processor.DefaultArguments)
        {
            if (arg is BoolArgument boolArg)
            {
                Arguments.Add(new BoolArgument
                {
                    Flag = boolArg.Flag,
                    Name = boolArg.Name,
                    Description = boolArg.Description,
                    Value = boolArg.Value
                });
            }
            else if (arg is NumericArgument numArg)
            {
                Arguments.Add(new NumericArgument
                {
                    Flag = numArg.Flag,
                    Name = numArg.Name,
                    Description = numArg.Description,
                    Value = numArg.Value,
                    MinValue = numArg.MinValue,
                    MaxValue = numArg.MaxValue
                });
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    public event Action<string[]?>? FilesSelected;

    public void SetSelectedFiles(string[]? files)
    {
        SelectedFiles = files;
        FilesSelected?.Invoke(files);
        
        if (files == null || files.Length == 0)
        {
            StatusMessage = "No files selected";
        }
        else
        {
            StatusMessage = $"{files.Length} file(s) selected";
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

        var processor = _sensorProcessorFactory.GetSensorProcessor(SelectedSensorTypes);

        try
        {
            IsProcessing = true;
            _sharedDataService.IsProcessingInBackground = true;
            
            StatusMessage = "Copying files...";
        
            var processingDirectory = _scriptService.GetScriptInputPath();
            var outputDirectory = AppConfig.OutputsDirectoryPath;
            
            processor.CopyFiles(SelectedFiles, processingDirectory, outputDirectory);
            
            StatusMessage = "Procesing files...";
            
            var (scriptSuccess, output, error) = await processor.ProcessAsync(Arguments);

            ScriptOutput = output;
            ShowScriptOutput = true;

            if (!scriptSuccess)
            {
                StatusMessage = $"Script execution failed: {error}";
                return;
            }

            StatusMessage = "Parsing results...";
            ParseResults();
            IsProcessing = false;
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
            _sharedDataService.IsProcessingInBackground = false;
        }
    }

    
    private async void ParseResults()
    {
        try
        {
            var parser = _resultParserFactory.GetParser(SensorType);
            var files = await parser.ParseResultsAsync(AppConfig.OutputsDirectoryPath);
            _sharedDataService.UpdateAllAnalyses(files);
        }
        catch (Exception e)
        {
            var dialog = new WarningDialogViewModel
            {
                Title = "Fehler",
                SubTitle =
                    "Die Ergebnisse konnten nicht geladen werden. Die hochgeladene Datei ist möglicherweise fehlerhaft.",
                CloseButtonText = "Abbrechen",
                OkButtonText = "OK"
            };
            await _dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(_mainViewModel, dialog);

            Console.WriteLine(e);
        }
    }

}