using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace ActiveSense.Desktop.Views;

public partial class MainView : AppWindow
{
    public MainView()
    {
        InitializeComponent();
        this.Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.Initialize();
            }
        };
    }
}