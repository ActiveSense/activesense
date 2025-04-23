using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ExportDialogViewModel : DialogViewModel
{
    [ObservableProperty] private bool _confirmed;
    
    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        Close();
    }
}