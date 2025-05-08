using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface IExporter
{
    Task<bool> ExportAsync(IAnalysis analysis, string outputPath, bool exportRawData);
}