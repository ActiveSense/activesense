namespace ActiveSense.Desktop.Import.Interfaces;

public interface IHeaderAnalyzer
{
    bool IsActivityCsv(string[] headers);
    bool IsSleepCsv(string[] headers);
}