using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Import.Interfaces;

public interface IFileParser
{
    AnalysisType DetermineAnalysisType(string[] headers);
    bool ParseCsvFile(string filePath, IAnalysis analysis);
    Task<IAnalysis> ParseCsvDirectoryAsync(string directory);
}