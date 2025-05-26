using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels.Dialogs;

public partial class InfoDialogViewModel : DialogViewModel
{
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string? _extendedMessage;
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _okButtonText = "Ok";
    [ObservableProperty] private string _subTitle = "";
    [ObservableProperty] private string _title = "Fehler";

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