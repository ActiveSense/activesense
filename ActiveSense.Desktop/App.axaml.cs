using System;
using System.Linq;
using ActiveSense.Desktop.Charts.Generators;
using ActiveSense.Desktop.Converters;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Interfaces;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.ViewModels.AnalysisPages;
using ActiveSense.Desktop.ViewModels.Charts;
using ActiveSense.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using ActivityPageViewModel = ActiveSense.Desktop.ViewModels.AnalysisPages.ActivityPageViewModel;
using BarChartViewModel = ActiveSense.Desktop.ViewModels.Charts.BarChartViewModel;
using SleepPageViewModel = ActiveSense.Desktop.ViewModels.AnalysisPages.SleepPageViewModel;

namespace ActiveSense.Desktop;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        collection.AddTransient<AnalysisSerializer>();
        
        // Register factories
        collection.AddSingleton<ResultParserFactory>();
        collection.AddSingleton<SensorProcessorFactory>();
        collection.AddSingleton<PageFactory>();
        collection.AddSingleton<ExporterFactory>();
        
        // Register processors
        collection.AddSingleton<GeneActivProcessor>();
        
        // Register exporters
        collection.AddSingleton<GeneActiveExporter>();
        
        // Register parsers
        collection.AddSingleton<GeneActiveResultParser>();

        // Register services
        collection.AddSingleton<IScriptService, RScriptService>();
        collection.AddSingleton<SharedDataService>();
        
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
        collection.AddTransient<DateToWeekdayConverter>();
        
        // Register charts
        collection.AddTransient<BarChartViewModel>();
        collection.AddTransient<PieChartViewModel>();
        collection.AddTransient<ChartColors>();

        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(x => name => name switch
        {
            ApplicationPageNames.Analyse => x.GetRequiredService<AnalysisPageViewModel>(),
            ApplicationPageNames.Sleep => x.GetRequiredService<SleepPageViewModel>(),
            ApplicationPageNames.Activity => x.GetRequiredService<ActivityPageViewModel>(),
            ApplicationPageNames.General => x.GetRequiredService<GeneralPageViewModel>(),
            _ => throw new InvalidOperationException(),
        });
        
        // Register processors 
        collection.AddSingleton<Func<SensorTypes, ISensorProcessor>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActivProcessor>(),
            _ => throw new InvalidOperationException(),
        });

        // Register parsers
        collection.AddSingleton<Func<SensorTypes, IResultParser>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveResultParser>(),
            _ => throw new InvalidOperationException(),
        });
        
        // Register exporters
        collection.AddSingleton<Func<SensorTypes, IExporter>>(sp => type => type switch
        {
            SensorTypes.GENEActiv => sp.GetRequiredService<GeneActiveExporter>(),
            _ => throw new InvalidOperationException(),
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