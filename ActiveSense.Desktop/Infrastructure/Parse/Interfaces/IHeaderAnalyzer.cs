namespace ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

public interface IHeaderAnalyzer
{
    bool IsActivityCsv(string[] headers);
    bool IsSleepCsv(string[] headers);
}