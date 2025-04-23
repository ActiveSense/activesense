using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private PageViewModel _activePage;
    [ObservableProperty] private string _title = "ActiveSense";

    [ObservableProperty] private DialogViewModel _dialog;
    private readonly PageFactory _pageFactory;

    /// <inheritdoc/>
    public MainViewModel(DialogViewModel dialog, PageFactory pageFactory)
    {
        _pageFactory = pageFactory;
        _dialog = dialog;
    }


    public async Task Initialize()
    {
        await Task.Run(() => {
            ActivePage = _pageFactory.GetPageViewModel(ApplicationPageNames.Analyse);
        });
    
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}