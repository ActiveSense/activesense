using System.Collections.ObjectModel;
using ActiveSense.Desktop.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels.Charts;

public abstract partial class ChartViewModel : PageViewModel
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

}