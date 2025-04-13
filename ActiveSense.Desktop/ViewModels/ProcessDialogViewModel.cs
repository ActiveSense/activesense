using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessDialogViewModel : DialogViewModel
{
    [ObservableProperty] private string _title = "Confirm";
    [ObservableProperty] private string _message = "Are you sure?";
    [ObservableProperty] private string _confirmText = "Yes";
    [ObservableProperty] private string _cancelText = "No";
    [ObservableProperty] private string _iconText = "\xe4e0";
    
    [ObservableProperty]
    private bool _confirmed;

    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }
}