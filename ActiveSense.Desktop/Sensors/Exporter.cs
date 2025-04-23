using System.Threading.Tasks;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Sensors;

public class GeneActiveExporter : IExporter
{
    public async Task<bool> ExportAsync(Analysis analysis, string outputPath)
    {
        return true;
    }
}