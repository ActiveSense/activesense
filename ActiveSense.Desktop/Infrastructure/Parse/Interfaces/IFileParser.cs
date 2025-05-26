using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

public interface IFileParser
{
    Task<IAnalysis> ParseCsvDirectoryAsync(string directory);
}