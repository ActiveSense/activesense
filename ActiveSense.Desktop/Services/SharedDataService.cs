using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Services;

public class SharedDataService
{
    public ObservableCollection<IAnalysis> SelectedAnalyses { get; } = new ObservableCollection<IAnalysis>();
    public ObservableCollection<IAnalysis> AllAnalyses { get; } = new();
    
    public event EventHandler? SelectedAnalysesChanged;
    public event EventHandler? AllAnalysesChanged;
    
    public void UpdateSelectedAnalyses(ObservableCollection<IAnalysis> analyses)
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in analyses)
        {
            SelectedAnalyses.Add(analysis);
        }
        
        SelectedAnalysesChanged?.Invoke(this, EventArgs.Empty);
    }
    public void UpdateAllAnalyses(IEnumerable<IAnalysis> newAnalyses)
    {
        var existingFilenames = new HashSet<string>(AllAnalyses.Select(a => a.FileName));
    
        foreach (var newAnalysis in newAnalyses)
        {
            var existingItem = AllAnalyses.FirstOrDefault(a => a.FileName == newAnalysis.FileName);
        
            if (existingItem != null)
            {
                newAnalysis.Exported = existingItem.Exported;
            
                int index = AllAnalyses.IndexOf(existingItem);
                AllAnalyses[index] = newAnalysis;
            }
            else
            {
                AllAnalyses.Add(newAnalysis);
            }
        }
    
        AllAnalysesChanged?.Invoke(this, EventArgs.Empty);
    }
}