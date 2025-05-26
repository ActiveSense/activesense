using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels.Dialogs;

public partial class WarningDialogViewModel : DialogViewModel
{
    [ObservableProperty] private string _closeButtonText = "Close";
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _message = "Are you sure?";
    [ObservableProperty] private string _okButtonText = "Ok";
    [ObservableProperty] private string _subTitle = "An error occurred during operation";
    [ObservableProperty] private string _title = "Warning";

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