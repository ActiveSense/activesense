using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Interfaces;

public interface IResultParser
{
    ApplicationPageNames[] GetAnalysisPages();
    Task<IEnumerable<Analysis>> ParseResultsAsync(string outputDirectory);
}