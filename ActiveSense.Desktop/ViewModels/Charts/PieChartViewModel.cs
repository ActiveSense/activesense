using LiveChartsCore;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class PieChartViewModel : ChartViewModel
{
    public string Title { get; set; }
    public ISeries[] PieSeries { get; set; }
}