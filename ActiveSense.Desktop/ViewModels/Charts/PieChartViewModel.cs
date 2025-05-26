using LiveChartsCore;

namespace ActiveSense.Desktop.ViewModels.Charts;

public class PieChartViewModel : ChartViewModel
{
    public ISeries[] PieSeries { get; set; } = [];
}