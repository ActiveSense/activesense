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
        var services = new ServiceCollection();
        
        // Register factories
        services.AddSingleton<IResultParserFactory, ResultParserFactory>();
        services.AddSingleton<ISensorProcessorFactory, SensorProcessorFactory>();
        
        // Register processors
        services.AddTransient<GeneActivProcessor>();
        // Add other processors as needed
        
        // Register parsers
        services.AddTransient<GeneActiveResultParser>();

        // Register services
        services.AddSingleton<IScriptService, RScriptService>();
        services.AddSingleton<SharedDataService>();
        
        // Register view models
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<DialogService>();
        services.AddTransient<DialogViewModel>();
        services.AddTransient<ProcessDialogViewModel>();
        services.AddTransient<AnalysisPageViewModel>();
        services.AddTransient<SleepPageViewModel>();
        services.AddTransient<ActivityPageViewModel>();
        services.AddTransient<GeneralPageViewModel>();
        services.AddTransient<ProcessViewModel>();
        
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