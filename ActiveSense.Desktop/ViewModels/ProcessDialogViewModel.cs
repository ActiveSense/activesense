using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.ViewModels.Dialogs;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Timer = System.Timers.Timer;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessDialogViewModel : DialogViewModel
{
    private readonly DialogService _dialogService;
    private readonly MainViewModel _mainViewModel;
    private readonly IPathService _pathService;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly SensorProcessorFactory _sensorProcessorFactory;
    private readonly ISharedDataService _sharedDataService;

    [ObservableProperty] private ObservableCollection<ScriptArgument> _arguments = new();
    private CancellationTokenSource? _cancellationTokenSource;
    [ObservableProperty] private string _cancelText = "Cancel";
    [ObservableProperty] private string _confirmText = "Confirm";
    private Timer? _countdownTimer;

    private TimeSpan _estimatedTime;
    [ObservableProperty] private bool _isProcessing;
    private DateTime _processingStartTime;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _scriptOutput = string.Empty;
    [ObservableProperty] private string[]? _selectedFiles;
    [ObservableProperty] private SensorTypes _selectedSensorTypes = SensorTypes.GENEActiv;

    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showProgress;
    [ObservableProperty] private bool _showScriptOutput;
    [ObservableProperty] private string _statusMessage = "No files selected";
    [ObservableProperty] private string _timeRemaining = string.Empty;
    [ObservableProperty] private string _title = "Sensordaten analysieren";
    [ObservableProperty] private string _processingInfo = "";

    public ProcessDialogViewModel(SensorProcessorFactory sensorProcessorFactory,
        ISharedDataService sharedDataService, ResultParserFactory resultParserFactory,
        IPathService pathService, DialogService dialogService, MainViewModel mainViewModel)
    {
        _resultParserFactory = resultParserFactory;
        _sensorProcessorFactory = sensorProcessorFactory;
        _sharedDataService = sharedDataService;
        _pathService = pathService;
        _dialogService = dialogService;
        _mainViewModel = mainViewModel;

        LoadDefaultInformation();
    }
    
    // public ProcessDialogViewModel() {}

    private void LoadDefaultInformation()
    {
        var processor = _sensorProcessorFactory.GetSensorProcessor(SelectedSensorTypes);
        ProcessingInfo = processor.ProcessingInfo;
        
        Arguments.Clear();

        foreach (var arg in processor.DefaultArguments)
            if (arg is BoolArgument boolArg)
                Arguments.Add(new BoolArgument
                {
                    Flag = boolArg.Flag,
                    Name = boolArg.Name,
                    Description = boolArg.Description,
                    Value = boolArg.Value
                });
            else if (arg is NumericArgument numArg)
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

    private void StartCountdown(TimeSpan estimatedTime)
    {
        _estimatedTime = estimatedTime;
        _processingStartTime = DateTime.Now;
        ShowProgress = true;

        _countdownTimer?.Dispose();

        _countdownTimer = new Timer(1000);
        _countdownTimer.Elapsed += UpdateCountdown;
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();

        UpdateCountdown(null, null);
    }

    private void UpdateCountdown(object? sender, ElapsedEventArgs? e)
    {
        var elapsed = DateTime.Now - _processingStartTime;
        var remaining = _estimatedTime - elapsed;

        if (remaining.TotalSeconds <= 0)
        {
            TimeRemaining = "Fertigstellen...";
            ProgressValue = 100;
        }
        else
        {
            TimeRemaining = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";

            ProgressValue = Math.Min(100, elapsed.TotalSeconds / _estimatedTime.TotalSeconds * 100);
        }

        Dispatcher.UIThread.Post(() =>
        {
            TimeRemaining = TimeRemaining;
            ProgressValue = ProgressValue;
        });
    }

    private void StopCountdown()
    {
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
        _countdownTimer = null;
        ShowProgress = false;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsProcessing && _cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            StatusMessage = "Cancelling operation...";
        }
        else
        {
            Close();
        }
    }

    [RelayCommand]
    private void Hide()
    {
        Close();
    }

    public void SetSelectedFiles(string[]? files)
    {
        SelectedFiles = files;

        if (files == null || files.Length == 0)
            StatusMessage = "No files selected";
        else
            StatusMessage = $"{files.Length} file(s) selected";
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

            StatusMessage = "Laufzeit wird berechnet...";
            var estimatedTime = await processor.GetEstimatedProcessingTimeAsync(SelectedFiles, Arguments.ToList());
            StartCountdown(estimatedTime);
                
            _sharedDataService.IsProcessingInBackground = true;
            _cancellationTokenSource = new CancellationTokenSource();

            StatusMessage = "Dateien werden kopiert...";

            var processingDirectory = _pathService.ScriptInputPath;
            var outputDirectory = _pathService.OutputDirectory;

            
            await processor.CopyFilesAsync(SelectedFiles, processingDirectory, outputDirectory);

            StatusMessage = "Dateien werden verarbeitet...";

            if (Directory.EnumerateFiles(processingDirectory).Any())
            {
                var (scriptSuccess, output) =
                    await processor.ProcessAsync(Arguments.ToList(), _cancellationTokenSource.Token);
                ScriptOutput = output;
                ShowScriptOutput = true;

                if (!scriptSuccess)
                {
                    var dialog = new InfoDialogViewModel()
                    {
                        Title = "Fehler",
                        Message = "Script Execution failed",
                        ExtendedMessage = output,
                        OkButtonText = "Schliessen",
                    };
                    await _dialogService.ShowDialog<MainViewModel, InfoDialogViewModel>(_mainViewModel, dialog);
                    return;
                }
            }


            StatusMessage = "Resultate werden analysiert...";
            StopCountdown();
            await ParseResults();
        }
        
        catch (OperationCanceledException)
        {
            Close();
        }
        
        catch (FileNotFoundException)
        {
            var dialog = new PathDialogViewModel()
            {
            };
            await _dialogService.ShowDialog<MainViewModel, PathDialogViewModel>(_mainViewModel, dialog);
        }
        
        catch (Exception ex)
        {
            var dialog = new InfoDialogViewModel
            {
                Title = "Fehler",
                Message =
                    "Ein Fehler ist aufgetreten. Bitte überprüfen Sie die Eingabedateien und versuchen Sie es erneut.",
                ExtendedMessage = ex.Message,
                OkButtonText = "Schliessen"
            };
            await _dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(_mainViewModel, dialog);
        }
        
        finally
        {
            StopCountdown();
            IsProcessing = false;
            _sharedDataService.IsProcessingInBackground = false;

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            Close();
        }
    }


    private async Task ParseResults()
    {
        var parser = _resultParserFactory.GetParser(SensorType);
        var files = await parser.ParseResultsAsync(_pathService.OutputDirectory);
        _sharedDataService.UpdateAllAnalyses(files);
    }
}