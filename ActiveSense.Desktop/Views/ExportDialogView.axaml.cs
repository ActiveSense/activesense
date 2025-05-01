using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Views;

public partial class ExportDialogView : UserControl
{
    public ExportDialogView()
    {
        InitializeComponent();
        
        // Wire up the file picker event when the DataContext is set
        this.DataContextChanged += (sender, e) =>
        {
            if (DataContext is ExportDialogViewModel viewModel)
            {
                viewModel.FilePickerRequested += ShowFilePickerAsync;
            }
        };
    }
    
    private async Task<string?> ShowFilePickerAsync(bool includeRawData)
    {
        var sp = GetStorageProvider();
        if (sp is null) return null;

        if (DataContext is ExportDialogViewModel viewModel)
        {
            if (viewModel.SelectedAnalysesCount == 1)
            {
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
                
                var result = await sp.SaveFilePickerAsync(new FilePickerSaveOptions()
                {
                    Title = "Save Analysis",
                    SuggestedFileName = fileName,
                    FileTypeChoices = fileTypes,
                    DefaultExtension = extension
                });
                
                return result?.Path.LocalPath;
            }
            else
            {
                viewModel.StatusMessage = "Please select exactly one analysis to export";
                return null;
            }
        }
        
        return null;
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }
    
    private List<FilePickerFileType> GetPdfFileType()
    {
        return new List<FilePickerFileType>
        {
            new FilePickerFileType("PDF Document")
            {
                Patterns = new[] { "*.pdf" },
                MimeTypes = new[] { "application/pdf" }
            }
        };
    }
    
    private List<FilePickerFileType> GetZipFileType()
    {
        return new List<FilePickerFileType>
        {
            new FilePickerFileType("ZIP Archive")
            {
                Patterns = new[] { "*.zip" },
                MimeTypes = new[] { "application/zip" }
            }
        };
    }
}