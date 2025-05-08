using System.Collections.Generic;
using ActiveSense.Desktop.Charts.DTOs;

namespace ActiveSense.Desktop.Core.Domain.Interfaces;
public interface IChartDataProvider
{
    ChartDataDTO GetStepsChartData();
    ChartDataDTO GetSleepDistributionChartData();
    ChartDataDTO GetMovementPatternChartData();
    IEnumerable<ChartDataDTO> GetActivityDistributionChartData();
    ChartDataDTO GetTotalSleepTimePerDayChartData();
    ChartDataDTO GetSedentaryChartData();
    ChartDataDTO GetLightActivityChartData();
    ChartDataDTO GetModerateActivityChartData();
    ChartDataDTO GetVigorousActivityChartData();
    ChartDataDTO GetSleepEfficiencyChartData();
    ChartDataDTO GetActivePeriodsChartData();

}
