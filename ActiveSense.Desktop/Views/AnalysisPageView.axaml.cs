using System.Collections.ObjectModel;
using System.Linq;
using ActiveSense.Desktop.Models;
using ActiveSense.Desktop.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop.Views;

public partial class AnalysisPageView : UserControl
{
    public AnalysisPageView()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is AnalysisPageViewModel viewModel && sender is ListBox listBox)
        {
            viewModel.SelectedAnalyses = new ObservableCollection<Analysis>(
                listBox.SelectedItems.Cast<Analysis>());
        }
    }
}