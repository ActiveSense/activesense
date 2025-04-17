using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : PageViewModel
{
    private readonly PageFactory _pageFactory;
    private readonly ResultParserFactory _resultParserFactory;
    private readonly SharedDataService _sharedDataService;
    [ObservableProperty] private ObservableCollection<Analysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showSpinner = true;

    public AnalysisPageViewModel(
        ResultParserFactory resultParserFactory,
        PageFactory pageFactory,
        SharedDataService sharedDataService)
    {
        _resultParserFactory = resultParserFactory;
        _sharedDataService = sharedDataService;
        _pageFactory = pageFactory;
        InitializePageCommand.Execute(null);
    }

    public ObservableCollection<TabItemTemplate> TabItems { get; } = [];

    partial void OnSelectedAnalysesChanged(ObservableCollection<Analysis> value)
    {
        _sharedDataService.UpdateSelectedAnalyses(value);
    }

    [RelayCommand]
    private async Task InitializePage()
    {
        Console.WriteLine("Loading result files...");
        try
        {
            var parser = _resultParserFactory.GetParser(SensorType);
            var files = await parser.ParseResultsAsync(AppConfig.OutputsDirectoryPath);
            ResultFiles.Clear();


            foreach (var file in files) ResultFiles.Add(file);
            
            foreach (var pageName in parser.GetAnalysisPages())
            {
                TabItems.Add(new TabItemTemplate($"{pageName.ToString()}", pageName,
                    _pageFactory.GetPageViewModel(pageName)));
                Console.WriteLine($"Loaded {pageName.ToString()}");
            }

            ShowSpinner = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class TabItemTemplate(string name, ApplicationPageNames pageName, ViewModelBase page)
{
    public string Name { get; set; } = name;
    public ApplicationPageNames PageName { get; set; } = pageName;
    public ViewModelBase Page { get; } = page;
}