using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Export.Interfaces;

public interface IAnalysisSerializer
{
    string ExportToBase64(IAnalysis analysis);
    IAnalysis ImportFromBase64(string base64);
}
