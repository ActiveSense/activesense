using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    private readonly DialogService _dialogService;
    private readonly PageFactory _pageFactory;
    private readonly IPathService _pathService;
    private readonly ISharedDataService _sharedDataService;
    [ObservableProperty] private PageViewModel _activePage;

    [ObservableProperty] private DialogViewModel _dialog;
    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private string _title = "ActiveSense";

    /// <inheritdoc />
    public MainViewModel(DialogViewModel dialog, PageFactory pageFactory, DialogService dialogService,
        IPathService pathService, ISharedDataService sharedDataService)
    {
        _pageFactory = pageFactory;
        _dialog = dialog;
        _dialogService = dialogService;
        _pathService = pathService;
        _sharedDataService = sharedDataService;
    }


    public async Task Initialize()
    {
        await Task.Run(() => { ActivePage = _pageFactory.GetPageViewModel(ApplicationPageNames.Analyse); });
        await CopyResourcesOnStartup();
    }

    private async Task CopyResourcesOnStartup()
    {
        await Task.Run(() => _pathService.CopyResources());
    }

    public async Task<bool> ConfirmOnClose()
    {
        try
        {
            if (_sharedDataService.HasUnsavedChanges())
            {
                var dialog = new WarningDialogViewModel
                {
                    Title = "Programm beenden?",
                    SubTitle = "Ungespeicherte Analysen gehen verloren.",
                    CloseButtonText = "Abbrechen",
                    OkButtonText = "Schliessen"
                };
                await _dialogService.ShowDialog<MainViewModel, WarningDialogViewModel>(this, dialog);

                if (!dialog.Confirmed) return false;
            }

            return true;
        }
        finally
        {
            try
            {
                _pathService.ClearDirectory(_pathService.OutputDirectory);
            }
            catch
            {
                // Ignore any errors during cleanup
            }
        }
    }
}