using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Views;

public partial class ExportDialogView : UserControl
{
    public ExportDialogView()
    {
        InitializeComponent();
        SaveAnalysesButton.Click += SelectFolderDialog;
    }
    
    private async void SelectFolderDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;

        var result = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Folder",
            AllowMultiple = false,
        });
        
        if (result != null && result.Count > 0)
        {
            var folderPath = result[0].Path.LocalPath;

            if (DataContext is ExportDialogViewModel viewModel)
            {
                viewModel.OutputPath = folderPath;
                if (OutputPathTextBlock != null)
                {
                    OutputPathTextBlock.Text = folderPath;
                }
            }
        }
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }
}