using System.Threading.Tasks;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Export.Interfaces;

public interface IPdfReportGenerator
{
    Task<bool> GeneratePdfReportAsync(IAnalysis analysis, string outputPath);
}