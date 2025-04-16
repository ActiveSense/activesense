using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Interfaces;

public interface IResultParser
{
    Task<IEnumerable<Analysis>> ParseResultsAsync(string outputDirectory);
}