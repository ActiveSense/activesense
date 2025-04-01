namespace ActiveSense.Desktop.Models;

public enum AnalysisType
{
    Sleep,
    Activity,
    Unknown,
}
public class AnalysisResult
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public AnalysisType AnalysisType { get; set; }
}