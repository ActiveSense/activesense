using DialogViewModel = ActiveSense.Desktop.ViewModels.Dialogs.DialogViewModel;

namespace ActiveSense.Desktop.Core.Services.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}