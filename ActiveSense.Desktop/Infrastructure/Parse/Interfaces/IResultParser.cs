using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Interfaces;

namespace ActiveSense.Desktop.Import.Interfaces;

public interface IResultParser
{
    ApplicationPageNames[] GetAnalysisPages();
    Task<IEnumerable<IAnalysis>> ParseResultsAsync(string outputDirectory);
}