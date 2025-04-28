using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : PageViewModel
{
    private readonly DialogService _dialogService;
    private readonly MainViewModel _mainViewModel;
    private readonly PageFactory _pageFactory;
    private readonly ProcessDialogViewModel _processDialogViewModel;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly SharedDataService _sharedDataService;
    private readonly ExportDialogViewModel _exportDialogViewModel;
    private bool _isInitialized = false;

    [ObservableProperty] private ObservableCollection<Analysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private TabItemTemplate _selectedTabItem;
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showSpinner = true;
    [ObservableProperty] private bool _showExportOption = true;

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
    }

    public ObservableCollection<TabItemTemplate> TabItems { get; } = [];

    partial void OnSelectedAnalysesChanged(ObservableCollection<Analysis> value)
    {
        _sharedDataService.UpdateSelectedAnalyses(value);
        ShowExportOption = SelectedAnalyses.Any();
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
            ResultFiles.Clear();
            
            var files = await parser.ParseResultsAsync(AppConfig.OutputsDirectoryPath);

            foreach (var file in files) ResultFiles.Add(file);

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
        ShowSpinner = false;
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
        await _dialogService.ShowDialog<MainViewModel, ExportDialogViewModel>(_mainViewModel, _exportDialogViewModel);
    }
}

public class TabItemTemplate(string name, ApplicationPageNames pageName, ViewModelBase page)
{
    public string Name { get; set; } = name;
    public ApplicationPageNames PageName { get; set; } = pageName;
    public ViewModelBase Page { get; } = page;
}