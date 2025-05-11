using System.Collections.Generic;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PathDialogViewModel = ActiveSense.Desktop.ViewModels.Dialogs.PathDialogViewModel;

namespace ActiveSense.Desktop.Views.Dialogs;

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
            Title = "Select R Installation (Rscript)",
            FileTypeFilter = GetFileTypes(),
            AllowMultiple = false
        });
        
        if (result.Count > 0)
        {
            string selectedPath = result[0].Path.LocalPath;
            
            if (DataContext is PathDialogViewModel viewModel)
            {
                viewModel.SelectedRInstallationPath = selectedPath;
            }
        }
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
            new FilePickerFileType("R Script Executable")
            {
                MimeTypes = ["application/x-executable"]
            },
            FilePickerFileTypes.All,
        ];
    }
}