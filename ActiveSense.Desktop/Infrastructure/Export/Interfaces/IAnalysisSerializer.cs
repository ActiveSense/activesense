using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface IAnalysisSerializer
{
    string ExportToBase64(IAnalysis analysis);
    IAnalysis ImportFromBase64(string base64);
}
