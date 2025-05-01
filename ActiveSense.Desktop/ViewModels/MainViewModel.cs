using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private PageViewModel _activePage;
    [ObservableProperty] private string _title = "ActiveSense";

    [ObservableProperty] private DialogViewModel _dialog;
    private readonly PageFactory _pageFactory;
    private readonly DialogService _dialogService;

    /// <inheritdoc/>
    public MainViewModel(DialogViewModel dialog, PageFactory pageFactory, DialogService dialogService)
    {
        _pageFactory = pageFactory;
        _dialog = dialog;
        _dialogService = dialogService;
    }


    public async Task Initialize()
    {
        await Task.Run(() => {
            ActivePage = _pageFactory.GetPageViewModel(ApplicationPageNames.Analyse);
        });
    
    }

    public async Task<bool> ConfirmOnClose()
    {
        var dialog = new WarningDialogViewModel
        {
            Title = "Programm beenden?",
            SubTitle = "Ungespeicherte Analysen gehen verloren.",
            CloseButtonText = "Abbrechen",
            OkButtonText = "Schliessen"
        };

        await _dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(this, dialog);
        
        return dialog.Confirmed;
    }
}