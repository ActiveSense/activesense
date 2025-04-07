using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop;
using ActiveSense.Desktop.Models;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : ViewModelBase
{
    private readonly IResultParserService _resultParserService;

    [ObservableProperty] private string _title = "Sleep";
    [ObservableProperty] private ObservableCollection<AnalysisResult> _resultFiles = new();

    public ObservableCollection<TabItemTemplate> TabItems { get; } = new ObservableCollection<TabItemTemplate>
    {
        new TabItemTemplate("Sleep", typeof(SleepPageViewModel), new SleepPageViewModel()),
        new TabItemTemplate("Activity", typeof(ActivityPageViewModel), new ActivityPageViewModel()),
        new TabItemTemplate("General", typeof(GeneralPageViewModel), new GeneralPageViewModel()),
    };

    public AnalysisPageViewModel()
    {
        _resultParserService = new ResultParserService();
        LoadResultFilesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadResultFiles()
    {
        Console.WriteLine("Loading result files...");
        try
        {
            var files = await _resultParserService.ParseResultsAsync(AppConfig.OutputsDirectoryPath);
            ResultFiles.Clear();

            foreach (var file in files)
            {
                ResultFiles.Add(file);
                Console.WriteLine(file.FileName);
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