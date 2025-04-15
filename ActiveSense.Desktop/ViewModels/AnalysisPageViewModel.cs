using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : PageViewModel
{
    private readonly ResultParserFactory _resultParserFactory;
    private readonly SharedDataService _sharedDataService;
    private readonly PageFactory _pageFactory;

    [ObservableProperty] private string _title = "Sleep";
    [ObservableProperty] private ObservableCollection<Analysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();
    [ObservableProperty] private SensorTypes _sensorType = SensorTypes.GENEActiv;
    [ObservableProperty] private bool _showSpinner = true;

    public ObservableCollection<TabItemTemplate> TabItems { get; }

    public AnalysisPageViewModel(
        ResultParserFactory resultParserFactory,
        PageFactory pageFactory,
        SharedDataService sharedDataService)
    {
        PageName = ApplicationPageNames.Analyse;
        _resultParserFactory = resultParserFactory;
        _sharedDataService = sharedDataService;
        _pageFactory = pageFactory;
        TabItems = [];
        InitializePageCommand.Execute(null);
    }

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

            
            foreach (var file in files)
            {
                ResultFiles.Add(file);
            }

            foreach (var pageName in parser.GetAnalysisPages())
            {
                TabItems.Add(new TabItemTemplate($"{pageName.ToString()}", pageName, _pageFactory.GetPageViewModel(pageName)));
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

public class TabItemTemplate
{
    public TabItemTemplate(string name, ApplicationPageNames pageName, ViewModelBase page)
    {
        Name = name;
        PageName = pageName;
        Page = page;
    }

    public string Name { get; set; }
    public ApplicationPageNames PageName { get; set; }
    public ViewModelBase Page { get; }
}