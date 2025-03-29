using ActiveSense.Desktop.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ActiveSense.Desktop.Views;

public partial class ProcessView : UserControl
{
    public ProcessView()
    {
        InitializeComponent();
        DataContext = new ProcessViewModel();
    }
}