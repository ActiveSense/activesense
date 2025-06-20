using System;
using System.Linq;
using ActiveSense.Desktop.Charts;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Core.Services;
using ActiveSense.Desktop.Core.Services.Interfaces;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Infrastructure.Export;
using ActiveSense.Desktop.Infrastructure.Export.Helpers;
using ActiveSense.Desktop.Infrastructure.Export.Interfaces;
using ActiveSense.Desktop.Infrastructure.Parse;
using ActiveSense.Desktop.Infrastructure.Parse.Helpers;
using ActiveSense.Desktop.Infrastructure.Parse.Interfaces;
using ActiveSense.Desktop.Infrastructure.Process;
using ActiveSense.Desktop.Infrastructure.Process.Helpers;
using ActiveSense.Desktop.Infrastructure.Process.Interfaces;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.AnalysisPages;
using ActiveSense.Desktop.ViewModels.Charts;
using ActiveSense.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ActivityPageViewModel = ActiveSense.Desktop.ViewModels.AnalysisPages.ActivityPageViewModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;
using DialogViewModel = ActiveSense.Desktop.ViewModels.Dialogs.DialogViewModel;
using SleepPageViewModel = ActiveSense.Desktop.ViewModels.AnalysisPages.SleepPageViewModel;

namespace ActiveSense.Desktop;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();


        // Register factories
        collection.AddSingleton<ResultParserFactory>();
        collection.AddSingleton<SensorProcessorFactory>();
        collection.AddSingleton<PageFactory>();
        collection.AddSingleton<ExporterFactory>();

        // Register processor components
        collection.AddSingleton<IScriptExecutor, ScriptExecutor>();
        collection.AddSingleton<IFileManager, FileManager>();
        collection.AddSingleton<IProcessingTimeEstimator, ProcessingTimeEstimator>();
        collection.AddSingleton<GeneActiveProcessor>();

        // Register export components
        collection.AddSingleton<ICsvExporter, CsvExporter>();
        collection.AddSingleton<IArchiveCreator, ArchiveCreator>();
        collection.AddSingleton<IPdfReportGenerator, PdfReportGenerator>();
        collection.AddSingleton<IAnalysisSerializer, AnalysisSerializer>();
        collection.AddSingleton<IExporter, GeneActiveExporter>();
        collection.AddSingleton<AnalysisSerializer>();
        collection.AddSingleton<GeneActiveExporter>();

        // Register parser components
        collection.AddSingleton<IHeaderAnalyzer, HeaderAnalyzer>();
        collection.AddSingleton<IFileParser, FileParser>();
        collection.AddSingleton<IPdfParser, PdfParser>();
        collection.AddSingleton<GeneActiveResultParser>();

        // Register services
        collection.AddSingleton<ISharedDataService, SharedDataService>();
        collection.AddSingleton<IPathService, PathService>();

        // Register view models
        collection.AddSingleton<MainViewModel>();
        collection.AddTransient<DialogService>();
        collection.AddTransient<DialogViewModel>();
        collection.AddTransient<ProcessDialogViewModel>();
        collection.AddTransient<AnalysisPageViewModel>();
        collection.AddTransient<SleepPageViewModel>();
        collection.AddTransient<ActivityPageViewModel>();
        collection.AddTransient<GeneralPageViewModel>();
        collection.AddTransient<ExportDialogViewModel>();

        // Register converters
        collection.AddSingleton<DateToWeekdayConverter>();

        // Logging
        collection.AddSingleton(Log.Logger);
        collection.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        // Register charts
        collection.AddTransient<BarChartViewModel>();
        collection.AddTransient<PieChartViewModel>();
        collection.AddSingleton<ChartColors>();

        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(x => name => name switch
        {
            ApplicationPageNames.Analyse => x.GetRequiredService<AnalysisPageViewModel>(),
            ApplicationPageNames.Schlaf => x.GetRequiredService<SleepPageViewModel>(),
            ApplicationPageNames.Aktivität => x.GetRequiredService<ActivityPageViewModel>(),
            ApplicationPageNames.Allgemein => x.GetRequiredService<GeneralPageViewModel>(),
            _ => throw new InvalidOperationException()
        });

        // Register processors 
        collection.AddSingleton<Func<SensorTypes, ISensorProcessor>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveProcessor>(),
            _ => throw new InvalidOperationException()
        });

        // Register parsers
        collection.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new InvalidOperationException()
        });

        // Register exporters
        collection.AddSingleton<Func<SensorTypes, IExporter>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveExporter>(),
            _ => throw new InvalidOperationException()
        });

        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit
            DisableAvaloniaDataAnnotationValidation();

            // Use dependency injection to get MainWindowViewModel
            desktop.MainWindow = new MainView
            {
                DataContext = services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}