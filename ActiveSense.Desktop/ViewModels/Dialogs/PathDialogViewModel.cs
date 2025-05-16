using System;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels.Dialogs;

public partial class PathDialogViewModel() : Dialogs.DialogViewModel
{
    [ObservableProperty] private string _closeButtonText = "Close";

    [ObservableProperty] private bool _confirmed;

    [ObservableProperty] private string _message = "Are you sure?";

    [ObservableProperty] private string _okButtonText = "Ok";

    [ObservableProperty] private string _selectedRInstallationPath = string.Empty;

    [ObservableProperty] private string _subTitle = "An error occurred during operation";

    [ObservableProperty] private string _title = "Warning";
    
    [ObservableProperty] private bool _errorOccurred = false;

    [RelayCommand]
    private void Ok()
    {
        Confirmed = true;
        if (!string.IsNullOrEmpty(SelectedRInstallationPath))
        {
            try
            {
                RPathStorage.SaveRPath(SelectedRInstallationPath);
                if (RPathStorage.TestRExecutable(RPathStorage.GetRPath()))
                {
                    Close();
                }
                else
                {
                    Message = "R konnte nicht gefunden werden. Bitte überprüfen Sie den Pfad erneut.";
                    ErrorOccurred = true;
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                ErrorOccurred = true;
            }
            finally
            {
                ErrorOccurred = false;
            }
        }
        else
        {
            Close();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Confirmed = false;
        Close();
    }
}