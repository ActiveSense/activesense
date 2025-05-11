using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.ViewModels.Dialogs;
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
    private readonly ISharedDataService _sharedDataService;
    [ObservableProperty] private bool _isProcessingInBackground;

    [ObservableProperty] private ObservableCollection<IAnalysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<IAnalysis> _selectedAnalyses = new();
    [ObservableProperty] private TabItemTemplate? _selectedTabItem = null;
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showExportOption = true;
    [ObservableProperty] private bool _showSpinner = true;

    public AnalysisPageViewModel(
        ResultParserFactory resultParserFactory,
        PageFactory pageFactory,
        ISharedDataService sharedDataService,
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
        _sharedDataService.AllAnalysesChanged += OnAnalysesChanged;
    }

    public ObservableCollection<TabItemTemplate> TabItems { get; } = [];

    async partial void OnSelectedAnalysesChanged(ObservableCollection<IAnalysis> value)
    {
        try
        {
            _sharedDataService.UpdateSelectedAnalyses(value);
            ShowExportOption = SelectedAnalyses.Count == 1;
        }
        catch (Exception e)
        {
            var dialog = new InfoDialogViewModel()
            {
                Title = "Fehler",
                Message = "Fehler beim Aktualisieren der Analysen.",
                OkButtonText = "Schliessen",
                ExtendedMessage = e.Message
            };
            await _dialogService.ShowDialog<MainViewModel, InfoDialogViewModel>(_mainViewModel, dialog);
        }
    }

    private void OnBackgroundProcessingChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => { IsProcessingInBackground = _sharedDataService.IsProcessingInBackground; });
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

        await Task.Run(() =>
        {
            foreach (var pageName in parser.GetAnalysisPages())
            {
                TabItems.Add(new TabItemTemplate(
                    $"{pageName.ToString()}",
                    pageName,
                    _pageFactory.GetPageViewModel(pageName)));
                Console.WriteLine($"Loaded {pageName.ToString()}");
            }

            if (TabItems.Count > 0) SelectedTabItem = TabItems[0];
            return Task.CompletedTask;
        });

        ShowSpinner = false;
    }



    [RelayCommand]
    private async Task TriggerProcessDialog()
    {
        await _dialogService.ShowDialog<MainViewModel, ProcessDialogViewModel>(_mainViewModel, _processDialogViewModel);
    }

    [RelayCommand]
    private async Task TriggerExportDialog()
    {
        if (SelectedAnalyses.Count != 1)
        {
            var dialog = new Dialogs.InfoDialogViewModel()
            {
                Title = "Export nicht möglich",
                Message = "Bitte wählen Sie eine Analyse zum Exportieren aus.",
                OkButtonText = "Schliessen"
            };
            await _dialogService.ShowDialog<MainViewModel, Dialogs.WarningDialogViewModel>(_mainViewModel, dialog);
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