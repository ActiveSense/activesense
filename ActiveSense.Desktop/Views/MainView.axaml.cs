using System.ComponentModel;
using Avalonia.Controls;

namespace ActiveSense.Desktop.Views;

public partial class MainView : Window 
{
    private bool _closingConfirmed = false;

    public MainView()
    {
        InitializeComponent();
        this.Loaded += (s, e) =>
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                viewModel.Initialize();
            }
        };

        this.Closing += OnWindowClosing;
    }

    private async void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (_closingConfirmed)
            return;

        e.Cancel = true;

        if (DataContext is ViewModels.MainViewModel viewModel)
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