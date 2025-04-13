using System;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Models;

namespace ActiveSense.Desktop.Services;

public class SharedDataService
{
    public ObservableCollection<Analysis> SelectedAnalyses { get; } = new ObservableCollection<Analysis>();
    
    public event EventHandler? SelectedAnalysesChanged;
    
    public void UpdateSelectedAnalyses(ObservableCollection<Analysis> analyses)
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in analyses)
        {
            SelectedAnalyses.Add(analysis);
        }
        
        // Notify subscribers that the data has changed
        SelectedAnalysesChanged?.Invoke(this, EventArgs.Empty);
    }
}