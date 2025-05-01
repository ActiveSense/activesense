using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : PageViewModel
{
    private readonly DialogService _dialogService;
    private readonly ExportDialogViewModel _exportDialogViewModel;
    private readonly MainViewModel _mainViewModel;
    private readonly PageFactory _pageFactory;
    private readonly ProcessDialogViewModel _processDialogViewModel;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly SharedDataService _sharedDataService;
    private bool _isInitialized = false;
    [ObservableProperty] private bool _isProcessingInBackground;

    [ObservableProperty] private ObservableCollection<IAnalysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = new();
    [ObservableProperty] private TabItemTemplate _selectedTabItem;
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showExportOption = true;
    [ObservableProperty] private bool _showSpinner = true;

    public AnalysisPageViewModel(
        ResultParserFactory resultParserFactory,
        PageFactory pageFactory,
        SharedDataService sharedDataService,
        DialogService dialogService,
        MainViewModel mainViewModel,
        ProcessDialogViewModel processDialogViewModel,
        ExportDialogViewModel exportDialogViewModel)
    {
        _resultParserFactory = resultParserFactory;
        _sharedDataService = sharedDataService;
        _pageFactory = pageFactory;
        _dialogService = dialogService;
        _mainViewModel = mainViewModel;
        _processDialogViewModel = processDialogViewModel;
        _exportDialogViewModel = exportDialogViewModel;
        ResultFiles = _sharedDataService.AllAnalyses;
        _sharedDataService.SelectedAnalysesChanged += OnAnalysesChanged;
        _sharedDataService.BackgroundProcessingChanged += OnBackgroundProcessingChanged;
    }

    public AnalysisPageViewModel()
    {
    }

    public ObservableCollection<TabItemTemplate> TabItems { get; } = [];

    partial void OnSelectedAnalysesChanged(ObservableCollection<IAnalysis> value)
    {
        _sharedDataService.UpdateSelectedAnalyses(value);
        ShowExportOption = SelectedAnalyses.Count == 1;
    }

    private void OnBackgroundProcessingChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => { IsProcessingInBackground = _sharedDataService.IsProcessingInBackground; });
        ParseResults();
    }

    private void OnAnalysesChanged(object? sender, EventArgs e)
    {
        ResultFiles = _sharedDataService.AllAnalyses;
    }

    [RelayCommand]
    public async Task Initialize()
    {
        ShowSpinner = true;
        Console.WriteLine("Loading result files...");
        TabItems.Clear();
        var parser = _resultParserFactory.GetParser(SensorType);

        await Task.Run(async () =>
        {
            foreach (var pageName in parser.GetAnalysisPages())
            {
                TabItems.Add(new TabItemTemplate(
                    $"{pageName.ToString()}",
                    pageName,
                    _pageFactory.GetPageViewModel(pageName)));
                Console.WriteLine($"Loaded {pageName.ToString()}");
            }

            // Select the first tab
            if (TabItems.Count > 0 && SelectedTabItem == null) SelectedTabItem = TabItems[0];
        });
        
        ParseResults();
        ShowSpinner = false;
    }

    private async void ParseResults()
    {
        var parser = _resultParserFactory.GetParser(SensorType);
        try
        {
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


    [RelayCommand]
    public async Task TriggerProcessDialog()
    {
        await _dialogService.ShowDialog<MainViewModel, ProcessDialogViewModel>(_mainViewModel, _processDialogViewModel);

        // Refresh data after dialog closes
        await Initialize();
    }

    [RelayCommand]
    public async Task TriggerExportDialog()
    {
        if (SelectedAnalyses.Count != 1)
        {
            var warningDialog = new WarningDialogViewModel
            {
                Title = "Export nicht möglich",
                SubTitle = "Bitte wählen Sie genau eine Analyse zum Exportieren aus.",
                CloseButtonText = "Schließen",
                OkButtonText = "OK"
            };
            await _dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(_mainViewModel, warningDialog);
            return;
        }
    
        await _dialogService.ShowDialog<MainViewModel, ExportDialogViewModel>(_mainViewModel, _exportDialogViewModel);
    }
}

public class TabItemTemplate(string name, ApplicationPageNames pageName, ViewModelBase page)
{
    public string Name { get; set; } = name;
    public ApplicationPageNames PageName { get; set; } = pageName;
    public ViewModelBase Page { get; } = page;
}