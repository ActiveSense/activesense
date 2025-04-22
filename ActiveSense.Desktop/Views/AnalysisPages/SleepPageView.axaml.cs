using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace ActiveSense.Desktop.Views.AnalysisPages;

public partial class SleepPageView : UserControl
{
    public SleepPageView()
    {
        InitializeComponent();

        // Attach handler for PointerWheelChanged as Tunneling event
        AddHandler(InputElement.PointerWheelChangedEvent, OnPreviewMouseWheel, RoutingStrategies.Tunnel);
    }

    private void OnPreviewMouseWheel(object? sender, PointerWheelEventArgs e)
    {
        if (MainScrollViewer != null)
        {
            // Apply scroll manually
            var delta = e.Delta.Y;
            MainScrollViewer.Offset = MainScrollViewer.Offset.WithY(MainScrollViewer.Offset.Y - delta * 20);
            e.Handled = true;
        }
    }

 
}