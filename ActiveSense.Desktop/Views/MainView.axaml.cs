using System.ComponentModel;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;

namespace ActiveSense.Desktop.Views;

public partial class MainView : Window
{
    private bool _closingConfirmed;

    public MainView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel viewModel) viewModel.Initialize();
        };

        Closing += OnWindowClosing;
    }

    private async void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (_closingConfirmed)
            return;

        e.Cancel = true;

        if (DataContext is MainViewModel viewModel)
        {
            var result = await viewModel.ConfirmOnClose();

            if (result)
            {
                _closingConfirmed = true;
                Close();
            }
        }
    }
}