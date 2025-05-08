using System.Collections.Generic;
using ActiveSense.Desktop.Charts.DTOs;
using ActiveSense.Desktop.Core.Domain.Models;

namespace ActiveSense.Desktop.Core.Domain.Interfaces;

public interface IAnalysis
{
    string FilePath { get; set; }
    string FileName { get; set; }
    bool Exported { get; set; }
    List<AnalysisTag> Tags { get; set; }
    void AddTag(string name, string color);
}




