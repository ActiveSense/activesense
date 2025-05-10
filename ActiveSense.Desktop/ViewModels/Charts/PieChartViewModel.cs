using System;
using LiveChartsCore;

namespace ActiveSense.Desktop.ViewModels.Charts;

public partial class PieChartViewModel : ChartViewModel
{
    public ISeries[] PieSeries { get; set; } = [];
}