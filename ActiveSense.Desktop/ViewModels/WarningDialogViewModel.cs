using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels
{
    public partial class WarningDialogViewModel : DialogViewModel
    {
        [ObservableProperty] private string _title = "Warning";
        [ObservableProperty] private string _subTitle = "An error occurred during operation";
        [ObservableProperty] private string _closeButtonText = "Close";
        [ObservableProperty] private string _okButtonText = "Ok";
        [ObservableProperty] private string _message = "Are you sure?";
        [ObservableProperty] private bool _confirmed;

        public WarningDialogViewModel()
        {
        }

        [RelayCommand]
        public void Cancel()
        {
            Confirmed = false;
            Close();
        }
        
        [RelayCommand]
        public void Ok()
        {
            Confirmed = true;
            Close();
        }
    }
}