using ActiveSense.Desktop.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private ApplicationPageNames _pageName;
}