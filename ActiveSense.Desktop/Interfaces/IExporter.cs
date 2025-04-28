using System.Threading.Tasks;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Interfaces;

public interface IExporter
{
    Task<bool> ExportAsync(Analysis analysis, string outputPath);
}