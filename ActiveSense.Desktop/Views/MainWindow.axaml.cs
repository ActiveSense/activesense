using System;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;

namespace ActiveSense.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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