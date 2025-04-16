using System.Collections.ObjectModel;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels.Charts;

public abstract partial class ChartViewModel : PageViewModel
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private ChartTypes _chartType;
    
    public abstract void UpdateChartData(ObservableCollection<Analysis> analyses);
}