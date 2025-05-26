using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Core.Domain.Interfaces;
using ActiveSense.Desktop.Core.Services.Interfaces;

namespace ActiveSense.Desktop.Core.Services;

public class SharedDataService : ISharedDataService
{
    private bool _isProcessingInBackground;
    public ObservableCollection<IAnalysis> SelectedAnalyses { get; } = new();
    public ObservableCollection<IAnalysis> AllAnalyses { get; } = new();

    public bool IsProcessingInBackground
    {
        get => _isProcessingInBackground;
        set
        {
            if (_isProcessingInBackground != value)
            {
                _isProcessingInBackground = value;
                BackgroundProcessingChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? BackgroundProcessingChanged;
    public event EventHandler? SelectedAnalysesChanged;
    public event EventHandler? AllAnalysesChanged;

    public void UpdateSelectedAnalyses(ObservableCollection<IAnalysis> analyses)
    {
        SelectedAnalyses.Clear();
        foreach (var analysis in analyses) SelectedAnalyses.Add(analysis);

        SelectedAnalysesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateAllAnalyses(IEnumerable<IAnalysis> newAnalyses)
    {
        foreach (var newAnalysis in newAnalyses)
        {
            var existingItem = AllAnalyses.FirstOrDefault(a => a.FileName == newAnalysis.FileName);

            if (existingItem != null)
            {
                newAnalysis.Exported = existingItem.Exported;
                var index = AllAnalyses.IndexOf(existingItem);
                AllAnalyses[index] = newAnalysis;
            }
            else
            {
                AllAnalyses.Add(newAnalysis);
            }
        }

        AllAnalysesChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasUnsavedChanges()
    {
        return AllAnalyses.Any(a => !a.Exported);
    }
}