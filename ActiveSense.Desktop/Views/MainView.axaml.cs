using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace ActiveSense.Desktop.Views;

public partial class MainView : AppWindow 
{
    public MainView()
    {
        InitializeComponent();
        
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }
}