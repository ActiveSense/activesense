using System.Collections.Generic;
using System.Threading.Tasks;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ActiveSense.Desktop.Views;

public partial class ExportDialogView : UserControl
{
    public ExportDialogView()
    {
        InitializeComponent();

        DataContextChanged += (sender, e) =>
        {
            if (DataContext is ExportDialogViewModel viewModel) viewModel.FilePickerRequested += ShowFilePickerAsync;
        };
    }

    private async Task<string?> ShowFilePickerAsync(bool includeRawData)
    {
        var sp = GetStorageProvider();
        if (sp is null) return null;

        if (DataContext is not ExportDialogViewModel { SelectedAnalysesCount: 1 } viewModel) return null;
        var selectedAnalysis = viewModel.GetFirstSelectedAnalysis();

        // Determine file extension and type based on export options
        string extension;
        List<FilePickerFileType> fileTypes;

        if (includeRawData)
        {
            extension = ".zip";
            fileTypes = GetZipFileType();
        }
        else
        {
            extension = ".pdf";
            fileTypes = GetPdfFileType();
        }

        var fileName = $"{selectedAnalysis.FileName}{extension}";

        var result = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Analysis",
            SuggestedFileName = fileName,
            FileTypeChoices = fileTypes,
            DefaultExtension = extension
        });

        return result?.Path.LocalPath;
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }

    private List<FilePickerFileType> GetPdfFileType()
    {
        return
        [
            new FilePickerFileType("PDF Document")
            {
                Patterns = ["*.pdf"],
                MimeTypes = ["application/pdf"]
            }
        ];
    }

    private List<FilePickerFileType> GetZipFileType()
    {
        return
        [
            new FilePickerFileType("ZIP Archive")
            {
                Patterns = ["*.zip"],
                MimeTypes = ["application/zip"]
            }
        ];
    }
}