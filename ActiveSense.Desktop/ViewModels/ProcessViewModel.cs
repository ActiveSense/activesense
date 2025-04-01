using System.Threading.Tasks;
using ActiveSense.Desktop.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class ProcessViewModel : ViewModelBase
{
    public Interaction<string, string[]?> SelectFilesInteraction { get; } = new Interaction<string, string[]?>();

    [ObservableProperty] private string[]? _selectedFiles;

    [RelayCommand]
    private async Task SelectFilesAsync()
    {
        SelectedFiles = await SelectFilesInteraction.HandleAsync("Hello test");
    }
}
