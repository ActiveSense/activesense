using System;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;
using ScottPlot.Avalonia;

namespace ActiveSense.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        double[] dataX = { 1, 2, 3, 4, 5 };
        double[] dataY = { 1, 4, 9, 16, 25 };

        var avaPlot1 = this.Find<AvaPlot>("AvaPlot1");
        avaPlot1.Plot.Add.Scatter(dataX, dataY);
        avaPlot1.Refresh();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainWindowViewModel viewModel)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel != null) viewModel.SetStorageProvider(topLevel.StorageProvider);
        }
    }
}