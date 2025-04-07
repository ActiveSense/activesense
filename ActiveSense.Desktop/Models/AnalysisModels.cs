using System.Collections.Generic;
using System.Diagnostics;
using CsvHelper.Configuration.Attributes;
namespace ActiveSense.Desktop.Models;

public enum AnalysisType
{
    Sleep,
    Activity,
    Unknown,
}
public abstract class AnalysisResult
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public AnalysisType AnalysisType { get; set; }
}

public class ActivityAnalysis : AnalysisResult
{
    public List<ActivityRecord> ActivityRecords = new List<ActivityRecord>();
}

public class SleepAnalysis : AnalysisResult
{
    public List<SleepRecord> SleepRecords = new List<SleepRecord>();
}

public class SleepRecord
{
    [Name("Night.Starting")]
    public string NightStarting { get; set; }
    
    [Name("Sleep.Onset.Time")]
    public string SleepOnsetTime { get; set; }
    
    [Name("Rise.Time")]
    public string RiseTime { get; set; }
    
    [Name("Total.Elapsed.Bed.Time")]
    public string TotalElapsedBedTime { get; set; }
    
    [Name("Total.Sleep.Time")]
    public string TotalSleepTime { get; set; }
    
    [Name("Total.Wake.Time")]
    public string TotalWakeTime { get; set; }
    
    [Name("Sleep.Efficiency")]
    public string SleepEfficiency { get; set; }
    
    [Name("Num.Active.Periods")]
    public string NumActivePeriods { get; set; }
    
    [Name("Median.Activity.Length")]
    public string MedianActivityLength { get; set; }
}

public class ActivityRecord
{
    [Name("Day.Number")]
    public string Day { get; set; }
    
    [Name("Steps")]
    public string Steps { get; set; }
    
    [Name("Non_Wear")]
    public string NonWear { get; set; }
    
    [Name("Sleep")]
    public string Sleep { get; set; }
    
    [Name("Sedentary")]
    public string Sedentary { get; set; }
    
    [Name("Light")]
    public string Light { get; set; }
    
    [Name("Moderate")]
    public string Moderate { get; set; }
    
    [Name("Vigorous")]
    public string Vigorous { get; set; }
}