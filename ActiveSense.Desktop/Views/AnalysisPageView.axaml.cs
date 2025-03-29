using ActiveSense.Desktop.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ActiveSense.Desktop.Views;

public partial class AnalysisPageView : UserControl
{
    public AnalysisPageView()
    {
        InitializeComponent();
        DataContext = new AnalysisPageViewModel();
    }
}