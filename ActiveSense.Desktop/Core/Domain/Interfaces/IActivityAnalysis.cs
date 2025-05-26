using System.Collections.Generic;
using ActiveSense.Desktop.Core.Domain.Models;

namespace ActiveSense.Desktop.Core.Domain.Interfaces;

public interface IActivityAnalysis : IAnalysis
{
    IReadOnlyCollection<ActivityRecord> ActivityRecords { get; }
    double[] StepsPerDay { get; }
    double[] ModerateActivity { get; }
    double[] VigorousActivity { get; }
    double[] LightActivity { get; }
    double[] SedentaryActivity { get; }
    double[] StepsPercentage { get; }
    double AverageSedentaryTime { get; }
    double AverageModerateActivity { get; }
    double AverageVigorousActivity { get; }
    double AverageLightActivity { get; }
    public string[] ActivityWeekdays();
    public string[] ActivityDates();

    void SetActivityRecords(IEnumerable<ActivityRecord> records);
}