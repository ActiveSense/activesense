using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainViewModel(DialogViewModel dialog, PageFactory pageFactory) : ViewModelBase, IDialogProvider
{
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private PageViewModel _activePage;
    [ObservableProperty] private ListItemTemplate? _selectedItem;
    [ObservableProperty] private string _title = "ActiveSense";
    
    [ObservableProperty] private DialogViewModel _dialog = dialog;

    partial void OnSelectedItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
        ActivePage = (PageViewModel)
            pageFactory.GetPageViewModel(ApplicationPageNames.Upload);
    }

    public ObservableCollection<ListItemTemplate> Items { get; } = new ObservableCollection<ListItemTemplate>
    {
        new ListItemTemplate("Analysis", typeof(AnalysisPageViewModel), "DataHistogramRegular"),
        new ListItemTemplate("Upload", typeof(ProcessPageViewModel), "DataBarVerticalAddRegular"),
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