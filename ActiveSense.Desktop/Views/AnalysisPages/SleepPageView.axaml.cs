using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ActiveSense.Desktop.Views.AnalysisPages;

public partial class SleepPageView : UserControl
{
    public SleepPageView()
    {
        InitializeComponent();

        // Attach handler for PointerWheelChanged as Tunneling event
        AddHandler(PointerWheelChangedEvent, OnPreviewMouseWheel, RoutingStrategies.Tunnel);
    }

    private void OnPreviewMouseWheel(object? sender, PointerWheelEventArgs e)
    {
        if (MainScrollViewer != null)
        {
            // Apply scroll manually
            var delta = e.Delta.Y;
            MainScrollViewer.Offset = MainScrollViewer.Offset.WithY(MainScrollViewer.Offset.Y - delta * 50);
            e.Handled = true;
        }
    }
}