using System;
using System.Linq;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Parse;

public class HeaderAnalyzer : IHeaderAnalyzer
{
    public bool IsActivityCsv(string[] headers)
    {
        var activityHeaders = new[]
        {
            "Day.Number", "Steps", "Non_Wear", "Sleep",
            "Sedentary", "Light", "Moderate", "Vigorous"
        };
        try
        {
            return headers.Intersect(activityHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
        }
        catch
        {
            return false;
        }
    }

    public bool IsSleepCsv(string[] headers)
    {
        var sleepHeaders = new[]
        {
            "Night.Starting", "Sleep.Onset.Time", "Rise.Time",
            "Total.Sleep.Time", "Sleep.Efficiency"
        };
        try
        {
            return headers.Intersect(sleepHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
        }
        catch
        {
            return false;
        }

    }
}