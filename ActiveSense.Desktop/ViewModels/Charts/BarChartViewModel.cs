using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace ActiveSense.Desktop.ViewModels.Charts
{
    public partial class BarChartViewModel : ChartViewModel
    {
        [ObservableProperty] private ISeries[] _series = [];
        [ObservableProperty] private ICartesianAxis[] _xAxes = [];
        [ObservableProperty] private ICartesianAxis[] _yAxes = [];
        [ObservableProperty] private LegendPosition _legendPosition = LegendPosition.Bottom;


        public BarChartViewModel()
        {
        }

        public BarChartViewModel(ISeries[] series, ICartesianAxis[] xAxes, ICartesianAxis[] yAxes)
        {
            Series = series;
            XAxes = xAxes;
            YAxes = yAxes;
        }
    }
}