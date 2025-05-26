using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActiveSense.Desktop.ViewModels.Dialogs;

public partial class PathDialogViewModel : DialogViewModel
{
    [ObservableProperty] private string _description =
        "ActiveSense benötigt R für die Verarbeitung von Sensordaten (.bin-Dateien). Bitte installieren Sie R oder geben Sie den Pfad zur vorhandenen Installation an.";

    [ObservableProperty] private string _downloadInstructions =
        "Installieren Sie R von der offiziellen Website und starten Sie die Anwendung erneut.";

    [ObservableProperty] private bool _isPathValid;

    [ObservableProperty] private bool _isTestingPath;

    [ObservableProperty] private string _rPath = string.Empty;

    [ObservableProperty] private bool _testResult;

    [ObservableProperty] private string _testResultColor = string.Empty;

    [ObservableProperty] private string _testResultMessage = string.Empty;

    public PathDialogViewModel()
    {
        RPath = RPathStorage.GetRPath();
        UpdatePathValidation();
    }

    public string TestButtonText => IsTestingPath ? "Teste..." : "Pfad testen";

    [RelayCommand]
    private async Task OpenWindowsDownload()
    {
        await OpenUrl("https://cran.r-project.org/bin/windows/base/");
    }

    [RelayCommand]
    private async Task OpenMacDownload()
    {
        await OpenUrl("https://cran.r-project.org/bin/macosx/");
    }

    [RelayCommand]
    private async Task OpenLinuxDownload()
    {
        await OpenUrl("https://cran.r-project.org/bin/linux/");
    }

    [RelayCommand]
    private async Task TestPath()
    {
        if (string.IsNullOrWhiteSpace(RPath))
        {
            ShowTestResult("Bitte geben Sie einen Pfad ein.", false);
            return;
        }

        IsTestingPath = true;
        TestResult = false;

        try
        {
            await Task.Delay(300); // Small delay for better UX

            var isValid = await Task.Run(() => RPathStorage.TestRExecutable(RPath));

            if (isValid)
            {
                ShowTestResult("R-Installation erfolgreich gefunden!", true);
                IsPathValid = true;
            }
            else
            {
                ShowTestResult("Ungültiger R-Pfad oder R nicht funktionsfähig.", false);
                IsPathValid = false;
            }
        }
        catch (Exception ex)
        {
            ShowTestResult($"Fehler beim Test: {ex.Message}", false);
            IsPathValid = false;
        }
        finally
        {
            IsTestingPath = false;
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(RPath)) RPathStorage.SaveRPath(RPath);

            Close();
        }
        catch (Exception ex)
        {
            ShowTestResult($"Fehler beim Speichern: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    partial void OnRPathChanged(string value)
    {
        UpdatePathValidation();
        TestResult = false;
    }

    private void UpdatePathValidation()
    {
        IsPathValid = !string.IsNullOrWhiteSpace(RPath) && File.Exists(RPath);
    }

    private void ShowTestResult(string message, bool isSuccess)
    {
        TestResultMessage = message;
        TestResultColor = isSuccess ? "{DynamicResource SemiColorSuccess}" : "{DynamicResource SemiColorError}";
        TestResult = true;
    }

    private static Task OpenUrl(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", url);
            else if (OperatingSystem.IsLinux()) Process.Start("xdg-open", url);
        }
        catch
        {
            // Ignore if we can't open the URL
        }

        return Task.CompletedTask;
    }
}