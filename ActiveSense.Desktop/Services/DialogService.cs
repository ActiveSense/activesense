using System.Threading.Tasks;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Services;

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
        await dialogViewModel.WaitAsnyc();
    }
}