using System;

namespace ActiveSense.Desktop.Converters;

public class DateToWeekdayConverter
{
    public DateTime ConvertStringToDate(string date)
    {
        try
        {
            return DateTime.Parse(date);
        }
        catch (Exception)
        {
            return DateTime.Parse("2000-01-01");
        }
    }

    public string ConvertDateToWeekday(string date)
    {
        var dateTime = ConvertStringToDate(date);
        try
        {
            return dateTime.ToString("dddd");
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to convert date {date} to weekday", e);
        }
    }
}