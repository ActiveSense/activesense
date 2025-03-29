using System;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class AnalysisPageViewModel : ViewModelBase
{

    [ObservableProperty] private string _title = "Sleep";

    public ObservableCollection<TabItemTemplate> TabItems { get; } = new ObservableCollection<TabItemTemplate>
    {
        new TabItemTemplate("Sleep", typeof(SleepPageViewModel), new SleepPageViewModel()),
        new TabItemTemplate("Activity", typeof(ActivityPageViewModel), new ActivityPageViewModel()),
        new TabItemTemplate("General", typeof(GeneralPageViewModel), new GeneralPageViewModel()),
    };

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