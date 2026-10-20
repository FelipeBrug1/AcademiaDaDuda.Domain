// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DependencyInjection;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.ViewModels;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "academia-da-duda.db3");
        builder.Services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = $"Data Source={databasePath}",
            DatabaseType = AppDatabaseType.Sqlite
        });
        builder.Services.AddSingleton<DatabaseSettingsStartup>();
        builder.Services.AddApplicationServices();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
