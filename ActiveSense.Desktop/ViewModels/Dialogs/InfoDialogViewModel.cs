using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels.Dialogs
{
    public partial class InfoDialogViewModel : Dialogs.DialogViewModel
    {
        [ObservableProperty] private string _title = "Fehler";
        [ObservableProperty] private string _subTitle = "";
        [ObservableProperty] private string _okButtonText = "Ok";
        [ObservableProperty] private string _message = "asdf asd fs df";
        [ObservableProperty] private string _extendedMessage = "";
        [ObservableProperty] private bool _confirmed;

        [RelayCommand]
        private void Cancel()
        {
            Confirmed = false;
            Close();
        }

        [RelayCommand]
        private void Ok()
        {
            Confirmed = true;
            Close();
        }
    }
}