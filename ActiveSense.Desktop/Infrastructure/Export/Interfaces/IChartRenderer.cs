using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface IChartRenderer
{
    byte[] RenderSleepDistributionChart(IChartDataProvider chartProvider);
    byte[] RenderMovementPatternChart(IChartDataProvider chartProvider);
    byte[] RenderStepsChart(IChartDataProvider chartProvider);
    byte[] RenderSleepWithEfficiencyChart(IChartDataProvider chartProvider);
    byte[] RenderStepsWithSleepEfficiencyChart(IChartDataProvider chartProvider);
    byte[] RenderActivityDistributionChart(IChartDataProvider chartProvider);
}