using System;
using System.Linq;
using ActiveSense.Desktop.Import.Interfaces;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Import.Implementations;

public class HeaderAnalyzer : IHeaderAnalyzer
{
    public bool IsActivityCsv(string[] headers)
    {
        var activityHeaders = new[]
        {
            "Day.Number", "Steps", "Non_Wear", "Sleep",
            "Sedentary", "Light", "Moderate", "Vigorous"
        };

        return headers.Intersect(activityHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
    }

    public bool IsSleepCsv(string[] headers)
    {
        var sleepHeaders = new[]
        {
            "Night.Starting", "Sleep.Onset.Time", "Rise.Time",
            "Total.Sleep.Time", "Sleep.Efficiency"
        };

        return headers.Intersect(sleepHeaders, StringComparer.OrdinalIgnoreCase).Count() >= 3;
    }
}