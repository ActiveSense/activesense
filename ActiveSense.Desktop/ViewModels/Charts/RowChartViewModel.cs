using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class RowChartViewModel() : ChartViewModel
{
    [ObservableProperty] private ISeries[] _series = [];
    [ObservableProperty] private ICartesianAxis[] _xAxes = [];
}