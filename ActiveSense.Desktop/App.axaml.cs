using System;
using System.Linq;
using ActiveSense.Desktop.Factories;
using ActiveSense.Desktop.Sensors;
using ActiveSense.Desktop.Services;
using ActiveSense.Desktop.ViewModels;
using ActiveSense.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;

namespace ActiveSense.Desktop;

public class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure LiveCharts
        LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
                .AddLightTheme());

        // Set up dependency injection
        var services = new ServiceCollection();
        
        // Register factories
        services.AddSingleton<IResultParserFactory, ResultParserFactory>();
        services.AddSingleton<ISensorProcessorFactory, SensorProcessorFactory>();
        
        // Register processors
        services.AddTransient<GeneActivProcessor>();
        // Add other processors as needed
        
        // Register parsers
        services.AddTransient<GeneActiveResultParser>();
        // Add other parsers as needed
        
        // Register view models
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AnalysisPageViewModel>();
        services.AddTransient<SleepPageViewModel>();
        services.AddTransient<ActivityPageViewModel>();
        services.AddTransient<GeneralPageViewModel>();
        services.AddTransient<ProcessViewModel>();
        
        // Build service provider
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit
            DisableAvaloniaDataAnnotationValidation();
            
            // Use dependency injection to get MainWindowViewModel
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
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