using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private IStorageProvider? _storageProvider;

    [ObservableProperty] private string _greeting = "Welcome to ActiveSense!";

    [ObservableProperty] private bool _isAnalyzing;

    public ObservableCollection<string> SelectedFiles { get; } = new();


    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    private async Task SelectFiles()
    {
        if (_storageProvider == null)
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    var topLevel = TopLevel.GetTopLevel(mainWindow);
                    if (topLevel != null) _storageProvider = topLevel.StorageProvider;
                }
            }

            if (_storageProvider == null)
            {
                Greeting = "Error: Storage provider not initialized";
                return;
            }
        }

        var options = new FilePickerOpenOptions
        {
            Title = "Select CSV or bin file",
            AllowMultiple = true,
            FileTypeFilter = new FilePickerFileType[]
            {
                new("CSV Files")
                {
                    Patterns = new[] { "*.csv" },
                    MimeTypes = new[] { "text/csv" }
                },
                new("Binary Files")
                {
                    Patterns = new[] { "*.bin" }
                }
            }
        };

        var files = await _storageProvider.OpenFilePickerAsync(options);

        SelectedFiles.Clear();

        if (files.Count >= 1)
        {
            foreach (var file in files) SelectedFiles.Add(file.Path.LocalPath);

            Greeting = $"Selected {files.Count} file(s)";

            if (files.Count > 0) await ProcessFileAsync(files[0]);
        }
    }

    private async Task ProcessFileAsync(IStorageFile file)
    {
        try
        {
            await using var stream = await file.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var fileContent = await streamReader.ReadToEndAsync();

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            if (extension == ".csv")
            {
                var lineCount = fileContent.Split('\n').Length;
                Greeting = $"CSV file with {lineCount} lines loaded";
            }
            else if (extension == ".bin")
            {
                Greeting = $"Binary file of {stream.Length} bytes loaded";
            }
        }
        catch (Exception ex)
        {
            Greeting = $"Error processing file: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task AnalyzeFiles()
    {
        Console.WriteLine("clicked");
        if (SelectedFiles.Count == 0)
        {
            Greeting = "No files selected for analysis";
            return;
        }
        
        try
        {
            IsAnalyzing = true;
            Greeting = "Analyzing files...";
        
            var csvFile = SelectedFiles.FirstOrDefault(f => Path.GetExtension(f).ToLowerInvariant() == ".csv");

            if (csvFile != null)
            {
                var fileContent = await File.ReadAllTextAsync(csvFile);
                Console.WriteLine(fileContent);
            }
            else
            {
                Greeting = "No CSV files found among the selected files";
            }
        }
        finally
        {
            IsAnalyzing = false;
            Greeting = "Analysis completed";
        }
    }
}