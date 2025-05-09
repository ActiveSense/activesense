using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class PathDialogViewModel : DialogViewModel
{
    [ObservableProperty] private bool _confirmed;

    [ObservableProperty] private string _message =
        "Are you sure?";

    [ObservableProperty] private string _okButtonText =
        "Ok";

    [ObservableProperty] private string _subTitle =
        "An error occurred during operation";

    [ObservableProperty] private string _title = "Warning";

    [RelayCommand]
    private void Ok()
    {
        Confirmed = true;
        Close();
    }
}