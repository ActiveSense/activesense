using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace ActiveSense.Desktop.Views;

public partial class ProcessDialogView : UserControl
{
    public ProcessDialogView()
    {
        InitializeComponent();
        OpenFileButton.Click += OpenFileDialog;
    }

    private async void OpenFileDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;

        var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            FileTypeFilter = GetFileTypes(),
            AllowMultiple = true,
        });

        if (result != null && result.Count > 0)
        {
            var filePaths = result.Select(file => file.Path.LocalPath).ToArray();

            if (DataContext is ProcessDialogViewModel viewModel)
            {
                viewModel.SetSelectedFiles(filePaths);
            }
        }
    }

    private IStorageProvider? GetStorageProvider()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            return topLevel?.StorageProvider;
        }

        List<FilePickerFileType>? GetFileTypes()
        {
            return
            [
                FilePickerFileTypes.All,
                FilePickerFileTypes.TextPlain
            ];
        }
    }