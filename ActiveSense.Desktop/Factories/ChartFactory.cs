using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.Charts;

namespace ActiveSense.Desktop.Factories;

public class ChartFactory(Func<ChartTypes, ChartViewModel> factory)
{
    public ChartViewModel GetChartViewModel(ChartTypes chartName) => factory.Invoke(chartName);
}
