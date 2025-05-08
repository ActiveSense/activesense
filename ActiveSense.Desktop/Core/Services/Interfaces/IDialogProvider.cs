using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Core.Services.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}