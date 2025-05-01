using System;
using System.Collections.Generic;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Interfaces;

public interface IAnalysis 
{
    string FilePath { get; set; }
    string FileName { get; set; }
    bool Exported { get; set; }
    List<AnalysisTag> Tags { get; set; }
    void AddTag(string name, string color);
}

public interface IActivityAnalysis : IAnalysis
{
    IReadOnlyCollection<ActivityRecord> ActivityRecords { get; }
    double[] StepsPerDay { get; }
    double[] ModerateActivity { get; }
    double[] VigorousActivity { get; }
    double[] LightActivity { get; }
    double [] SedentaryActivity { get; }
    double[] StepsPercentage { get; }
    double AverageSedentaryTime { get; }
    double AverageModerateActivity { get; }
    double AverageVigorousActivity { get; }
    double AverageLightActivity { get; }
    public string[] ActivityWeekdays();
    
    void SetActivityRecords(IEnumerable<ActivityRecord> records);
}

public interface ISleepAnalysis : IAnalysis
{
    IReadOnlyCollection<SleepRecord> SleepRecords { get; }
    
    double TotalSleepTime { get; }
    double TotalWakeTime { get; }
    double AverageSleepTime { get; }
    double AverageWakeTime { get; }
    double[] SleepEfficiency { get; }
    double[] TotalSleepTimePerDay { get; }
    
    string[] SleepWeekdays();
    
    
    void SetSleepRecords(IEnumerable<SleepRecord> records);
}

public interface IChartDataProvider
{
    ChartDataDTO GetStepsChartData();
    ChartDataDTO GetSleepChartData();
    ChartDataDTO GetMovementPatternChartData();
    IEnumerable<ChartDataDTO> GetActivityDistributionChartData();
    ChartDataDTO GetTotalSleepTimePerDayChartData();
    ChartDataDTO GetSedentaryChartData();
    ChartDataDTO GetLightActivityChartData();
    ChartDataDTO GetModerateActivityChartData();
    ChartDataDTO GetVigorousActivityChartData();

}