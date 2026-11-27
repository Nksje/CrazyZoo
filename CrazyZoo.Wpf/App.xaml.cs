using System.Windows;
using CrazyZoo.Core.Persistence;
using CrazyZoo.Wpf.Services;
using CrazyZoo.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CrazyZoo.Wpf;

// App koostab objektid ühe korra (DI). StartupUri on App.xaml-ist eemaldatud.
public partial class App : Application
{
    private ServiceProvider? _provider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ConfigureServices();

        MainWindow = _provider!.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    private void ConfigureServices()
    {
        AppSettings settings = AppSettings.Load();
        var services = new ServiceCollection();

        // Siin ja ainult siin valitakse salvestusteostus (appsettings.json → "Storage")
        if (string.Equals(settings.Storage, "Sql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IWorldStorage>(new SqlWorldStorage(settings.ConnectionString));
            services.AddSingleton<IStorageLocationPicker>(new FixedLocationPicker("default"));
        }
        else
        {
            services.AddSingleton<JsonWorldStorage>();
            services.AddSingleton<XmlWorldStorage>();
            services.AddSingleton<IWorldStorage, FileWorldStorage>();
            services.AddSingleton<IStorageLocationPicker, FileDialogLocationPicker>();
        }

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _provider = services.BuildServiceProvider();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _provider?.Dispose();
        base.OnExit(e);
    }
}
