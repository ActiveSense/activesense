using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isPaneOpen = true;
    
    [ObservableProperty] private ViewModelBase _activePage = new AnalysisPageViewModel();
    
    [ObservableProperty] private ListItemTemplate? _selectedItem;
    
    [ObservableProperty] private string _title = "ActiveSense";
    
    partial void OnSelectedItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
        ActivePage = (ViewModelBase)Activator.CreateInstance(value.ModelType)!;
    }
    
    public ObservableCollection<ListItemTemplate> Items { get; } = new ObservableCollection<ListItemTemplate>
    {
        new ListItemTemplate("Analysis", typeof(AnalysisPageViewModel), "DataHistogramRegular"),
        new ListItemTemplate("Upload", typeof(ProcessViewModel), "DataBarVerticalAddRegular"),
    };

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(string name, Type modelType, string icon)
    {
        Name = name;
        ModelType = modelType;
        Application.Current!.TryFindResource(icon, out var res);
        ListItemIcon = (StreamGeometry)res;
    }
  
    public string Name { get; set; }
    public Type ModelType { get; set; }
    public StreamGeometry ListItemIcon { get; }
}