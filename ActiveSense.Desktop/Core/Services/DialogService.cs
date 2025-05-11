using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.ViewModels;
using DialogViewModel = ActiveSense.Desktop.ViewModels.Dialogs.DialogViewModel;

namespace ActiveSense.Desktop.Core.Services;

public class DialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host, DialogViewModel dialogViewModel)
        where THost : IDialogProvider
        where TDialogViewModel : ViewModelBase
    {
        // Set the dialog view model to the host
        host.Dialog = dialogViewModel;
        dialogViewModel.Show();

        // Wait for the dialog to be closed
        await dialogViewModel.WaitAsync();
    }
}