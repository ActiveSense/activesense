using System.Collections.Generic;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Tests.ExportTests.Mock
{
    internal class InvalidAnalysis : IAnalysis
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public bool Exported { get; set; } = false;
        public List<AnalysisTag> Tags { get; set; } = new();
        
        public void AddTag(string name, string color)
        {
            // Do nothing
        }
    }
}
