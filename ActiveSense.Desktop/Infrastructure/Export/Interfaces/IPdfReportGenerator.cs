using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Export.Interfaces;

public interface IPdfReportGenerator
{
    Task<bool> GeneratePdfReportAsync(IAnalysis analysis, string outputPath);
}