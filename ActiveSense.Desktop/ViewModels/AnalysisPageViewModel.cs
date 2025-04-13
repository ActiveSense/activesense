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
    private readonly IResultParserFactory _resultParserFactory;
    private readonly SharedDataService _sharedDataService;
    private readonly PageFactory _pageFactory;

    [ObservableProperty] private string _title = "Sleep";
    [ObservableProperty] private ObservableCollection<Analysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();

    public ObservableCollection<TabItemTemplate> TabItems { get; }

    public AnalysisPageViewModel(
        IResultParserFactory resultParserFactory,
        PageFactory pageFactory,
        SharedDataService sharedDataService)
    {
        PageName = ApplicationPageNames.Analyse;
        _resultParserFactory = resultParserFactory;
        _sharedDataService = sharedDataService;
        _pageFactory = pageFactory;

        TabItems = new ObservableCollection<TabItemTemplate>
        {
            new TabItemTemplate("Sleep", ApplicationPageNames.Sleep,
                _pageFactory.GetPageViewModel(ApplicationPageNames.Sleep)),
            new TabItemTemplate("Activity", ApplicationPageNames.Activity,
                _pageFactory.GetPageViewModel(ApplicationPageNames.Activity)),
            new TabItemTemplate("General", ApplicationPageNames.General,
                _pageFactory.GetPageViewModel(ApplicationPageNames.General)),
        };

        LoadResultFilesCommand.Execute(null);
    }

    partial void OnSelectedAnalysesChanged(ObservableCollection<Analysis> value)
    {
        _sharedDataService.UpdateSelectedAnalyses(value);
    }

    [RelayCommand]
    private async Task LoadResultFiles()
    {
        Console.WriteLine("Loading result files...");
        try
        {
            var parser = _resultParserFactory.GetParser(SensorType.GENEActiv);
            var files = await parser.ParseResultsAsync(AppConfig.OutputsDirectoryPath);
            ResultFiles.Clear();

            foreach (var file in files)
            {
                ResultFiles.Add(file);
            }
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