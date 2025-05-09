using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ActiveSense.Desktop.Views;

public partial class PathDialogView : UserControl
{
    public PathDialogView()
    {
        InitializeComponent();
        OpenFileButton.Click += OpenFileDialog;
    }

    private async void OpenFileDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;
        var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            FileTypeFilter = GetFileTypes(),
            AllowMultiple = false
        });
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }

    private List<FilePickerFileType>? GetFileTypes()
    {
        return
        [
            FilePickerFileTypes.All,
        ];
    }
}