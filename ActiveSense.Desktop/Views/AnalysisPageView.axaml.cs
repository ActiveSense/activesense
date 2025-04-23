using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;

namespace ActiveSense.Desktop.Views;

public partial class AnalysisPageView : UserControl
{
    public AnalysisPageView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (DataContext is AnalysisPageViewModel viewModel) viewModel.Initialize();
        };
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is AnalysisPageViewModel viewModel && sender is ListBox listBox)
            viewModel.SelectedAnalyses = new ObservableCollection<Analysis>(
                listBox.SelectedItems.Cast<Analysis>());
    }
}