using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private PageViewModel _activePage;
    [ObservableProperty] private string _title = "ActiveSense";

    [ObservableProperty] private Dialogs.DialogViewModel _dialog;
    private readonly PageFactory _pageFactory;
    private readonly DialogService _dialogService;
    private readonly IPathService _pathService;

    /// <inheritdoc/>
    public MainViewModel(Dialogs.DialogViewModel dialog, PageFactory pageFactory, DialogService dialogService, IPathService pathService)
    {
        _pageFactory = pageFactory;
        _dialog = dialog;
        _dialogService = dialogService;
        _pathService = pathService;
    }


    public async Task Initialize()
    {
        await Task.Run(() =>
        {
            ActivePage = _pageFactory.GetPageViewModel(ApplicationPageNames.Analyse);
        });
        await CopyResourcesOnStartup();
    }

    private async Task CopyResourcesOnStartup()
    {
        await Task.Run(() => _pathService.CopyResources());
    }

    public async Task<bool> ConfirmOnClose()
    {
        var dialog = new Dialogs.WarningDialogViewModel
        {
            Title = "Programm beenden?",
            SubTitle = "Ungespeicherte Analysen gehen verloren.",
            CloseButtonText = "Abbrechen",
            OkButtonText = "Schliessen"
        };

        await _dialogService.ShowDialog<MainViewModel, Dialogs.WarningDialogViewModel>(this, dialog);
        
        // Clear the output directory on exit
        if (dialog.Confirmed)
        {
            _pathService.ClearDirectory(_pathService.OutputDirectory);
        }
        
        return dialog.Confirmed;
    }
}