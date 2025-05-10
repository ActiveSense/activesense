using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Enums;

namespace ActiveSense.Desktop.Infrastructure.Parse.Interfaces;

public interface IResultParser
{
    ApplicationPageNames[] GetAnalysisPages();
    Task<IEnumerable<IAnalysis>> ParseResultsAsync(string outputDirectory);
}