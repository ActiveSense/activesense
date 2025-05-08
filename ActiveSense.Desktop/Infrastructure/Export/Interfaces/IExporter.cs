using System.Threading.Tasks;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Export.Interfaces;

public interface IExporter
{
    Task<bool> ExportAsync(IAnalysis analysis, string outputPath, bool exportRawData);
}