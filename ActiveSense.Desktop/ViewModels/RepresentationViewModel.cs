using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class RepresentationViewModel : ViewModelBase
{
    [ObservableProperty] private string _title = "Representation";
}