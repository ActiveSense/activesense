using System.ComponentModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace ActiveSense.Desktop.Views;

public partial class MainView : AppWindow
{
    private bool _closingConfirmed = false;
    
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
        
        this.Closing += OnWindowClosing;
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