using System;
using System.Globalization;

namespace ActiveSense.Desktop.Converters;

public class DateToWeekdayConverter
{
    private readonly CultureInfo _germanCulture = new CultureInfo("de-DE");

    private DateTime ConvertStringToDate(string date)
    {
        try
        {
            return DateTime.Parse(date);
        }
        catch (Exception)
        {
            throw new Exception("Invalid date format: " + date);
        }
    }

    public string ConvertDateToWeekday(string date)
    {
        var dateTime = ConvertStringToDate(date);
        try
        {
            // Use German culture to get the weekday name
            return dateTime.ToString("dddd", _germanCulture);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to convert date {date} to weekday", e);
        }
    }
}