using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Enums;

namespace ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

public interface IFileParser
{
    AnalysisType DetermineAnalysisType(string[] headers);
    bool ParseCsvFile(string filePath, IAnalysis analysis);
    Task<IAnalysis> ParseCsvDirectoryAsync(string directory);
}