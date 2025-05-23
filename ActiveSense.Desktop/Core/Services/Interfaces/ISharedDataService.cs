using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActiveSense.Desktop.Core.Domain.Interfaces;

namespace ActiveSense.Desktop.Core.Services.Interfaces
{
    public interface ISharedDataService
    {
        ObservableCollection<IAnalysis> SelectedAnalyses { get; }
        ObservableCollection<IAnalysis> AllAnalyses { get; }
        bool IsProcessingInBackground { get; set; }
        bool HasUnsavedChanges();
        
        event EventHandler BackgroundProcessingChanged;
        event EventHandler SelectedAnalysesChanged;
        event EventHandler AllAnalysesChanged;
        
        void UpdateSelectedAnalyses(ObservableCollection<IAnalysis> analyses);
        void UpdateAllAnalyses(IEnumerable<IAnalysis> newAnalyses);
    }
}