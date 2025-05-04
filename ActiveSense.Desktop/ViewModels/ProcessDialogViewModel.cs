using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.HelperClasses;
using ActiveSense.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Timer = System.Timers.Timer;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessDialogViewModel : DialogViewModel
{
    private readonly DialogService _dialogService;
    private readonly MainViewModel _mainViewModel;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly IScriptService _scriptService;
    private readonly SensorProcessorFactory _sensorProcessorFactory;
    private readonly SharedDataService _sharedDataService;

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

    public ProcessDialogViewModel(SensorProcessorFactory sensorProcessorFactory, IScriptService scriptService,
        SharedDataService sharedDataService, ResultParserFactory resultParserFactory, DialogService dialogService,
        MainViewModel mainViewModel)
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
            TimeRemaining = "Finalizing...";
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

    public event Action<string[]?>? FilesSelected;

    public void SetSelectedFiles(string[]? files)
    {
        SelectedFiles = files;
        FilesSelected?.Invoke(files);

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
        var estimatedTime = processor.GetEstimatedProcessingTime(SelectedFiles);
        StartCountdown(estimatedTime);

        try
        {
            IsProcessing = true;
            _sharedDataService.IsProcessingInBackground = true;
            _cancellationTokenSource = new CancellationTokenSource();

            StatusMessage = "Copying files...";

            var processingDirectory = _scriptService.GetScriptInputPath();
            var outputDirectory = AppConfig.OutputsDirectoryPath;

            processor.CopyFiles(SelectedFiles, processingDirectory, outputDirectory);

            StatusMessage = "Procesing files...";

            var (scriptSuccess, output, error) = await processor.ProcessAsync(Arguments, _cancellationTokenSource.Token);

            ScriptOutput = output;
            ShowScriptOutput = true;

            if (!scriptSuccess)
            {
                StatusMessage = $"Script execution failed: {error}";
                return;
            }

            StatusMessage = "Parsing results...";
            ParseResults();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Operation was cancelled";
            ScriptOutput = "Processing was cancelled by user.";
            ShowScriptOutput = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ScriptOutput = ex.ToString();
            ShowScriptOutput = true;
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
        var files = await parser.ParseResultsAsync(AppConfig.OutputsDirectoryPath);
        _sharedDataService.UpdateAllAnalyses(files);
    }
}