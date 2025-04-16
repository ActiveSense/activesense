using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveSense.Desktop.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ActiveSense.Desktop.Views;

public partial class ProcessPageView : UserControl
{
    public ProcessPageView()
    {
        InitializeComponent();
    }
    
    // Stores a reference to the disposable in order to clean it up if needed
    IDisposable? _selectFilesInteractionDisposable;

    /*
     * Needs refactoring
     */
    protected override void OnDataContextChanged(EventArgs e)
    {
        // Dispose any old handler
        _selectFilesInteractionDisposable?.Dispose();

        if (DataContext is ProcessPageViewModel vm)
        {
            // register the interaction handler
            _selectFilesInteractionDisposable =
                vm.SelectFilesInteractionService.RegisterHandler(InteractionHandler);
        }

        base.OnDataContextChanged(e);
    }
    
    private async Task<string[]?> InteractionHandler(string input)
    {
        // Get a reference to our TopLevel (in our case the parent Window)
        var topLevel = TopLevel.GetTopLevel(this);

        // Try to get the files
        var storageFiles = await topLevel!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                AllowMultiple = true,
                Title = input
            });

        if (storageFiles == null || !storageFiles.Any())
        {
            return null;
        }
    
        var fullPaths = new List<string>();
    
        foreach (var file in storageFiles)
        {
            try
            {
                var path = file.Path.LocalPath;
                fullPaths.Add(path);
            
                Console.WriteLine($"Selected file path: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting path for file {file.Name}: {ex.Message}");
            }
        }

        return fullPaths.ToArray();
    }
}