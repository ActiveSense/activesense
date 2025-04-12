using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.Sensors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : ViewModelBase
{
    private readonly IResultParserFactory _resultParserFactory;

    [ObservableProperty] private string _title = "Sleep";
    [ObservableProperty] private ObservableCollection<Analysis> _resultFiles = new();
    [ObservableProperty] private ObservableCollection<Analysis> _selectedAnalyses = new();

    public ObservableCollection<TabItemTemplate> TabItems { get; } 

    public AnalysisPageViewModel(
        IResultParserFactory resultParserFactory,
        IServiceProvider serviceProvider)
    {
        _resultParserFactory = resultParserFactory;
        
        // Create TabItems using dependency injection
        TabItems = new ObservableCollection<TabItemTemplate>
        {
            new TabItemTemplate("Sleep", typeof(SleepPageViewModel), 
                serviceProvider.GetRequiredService<SleepPageViewModel>()),
            new TabItemTemplate("Activity", typeof(ActivityPageViewModel), 
                serviceProvider.GetRequiredService<ActivityPageViewModel>()),
            new TabItemTemplate("General", typeof(GeneralPageViewModel), 
                serviceProvider.GetRequiredService<GeneralPageViewModel>()),
        };
        
        LoadResultFilesCommand.Execute(null);
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
    public TabItemTemplate(string name, Type modelType, ViewModelBase page)
    {
        Name = name;
        ModelType = modelType;
        Page = page;
    }

    public string Name { get; set; }
    public Type ModelType { get; set; }

    public ViewModelBase Page { get; }
}