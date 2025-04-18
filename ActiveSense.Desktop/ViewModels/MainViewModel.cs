using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private PageViewModel _activePage;
    [ObservableProperty] private ListItemTemplate? _selectedItem;
    [ObservableProperty] private string _title = "ActiveSense";

    [ObservableProperty] private DialogViewModel _dialog;
    private readonly PageFactory _pageFactory;

    /// <inheritdoc/>
    public MainViewModel(DialogViewModel dialog, PageFactory pageFactory)
    {
        _pageFactory = pageFactory;
        _dialog = dialog;
    }

    [RelayCommand]
    private void SelectItem(ListItemTemplate item)
    {
        // First, set all items to not selected
        foreach (var menuItem in Items)
        {
            menuItem.IsSelected = false;
        }

        // Set the selected item
        item.IsSelected = true;

        // Update the SelectedItem property (which should trigger ActivePage update via OnSelectedItemChanged)
        SelectedItem = item;
    }

    partial void OnSelectedItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
        ActivePage = (PageViewModel)
            _pageFactory.GetPageViewModel(value.PageName);
    }

    public ObservableCollection<ListItemTemplate> Items { get; } = new ObservableCollection<ListItemTemplate>
    {
        new ListItemTemplate("Analysis", ApplicationPageNames.Analyse, "DataHistogramRegular"),
        new ListItemTemplate("Upload", ApplicationPageNames.Upload, "DataBarVerticalAddRegular"),
    };

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(string name, ApplicationPageNames pageName, string icon)
    {
        Name = name;
        PageName = pageName;
        Application.Current!.TryFindResource(icon, out var res);
        ListItemIcon = (StreamGeometry)res;
        IsSelected = false;  // Add this line
    }

    public string Name { get; set; }
    public ApplicationPageNames PageName { get; set; }
    public StreamGeometry ListItemIcon { get; }
    public bool IsSelected { get; set; }  // Add this property
}