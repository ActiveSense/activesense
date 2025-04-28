using System.Linq;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Models;

public static class AnalysisExtensions
{
    public static bool HasSleepData(this IAnalysis analysis)
    {
        return analysis is ISleepAnalysis sleepAnalysis && 
               sleepAnalysis.SleepRecords != null && 
               sleepAnalysis.SleepRecords.Any();
    }
    
    public static bool HasActivityData(this IAnalysis analysis)
    {
        return analysis is IActivityAnalysis activityAnalysis && 
               activityAnalysis.ActivityRecords != null && 
               activityAnalysis.ActivityRecords.Any();
    }
}