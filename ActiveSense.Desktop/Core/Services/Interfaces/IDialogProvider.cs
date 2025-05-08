using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}