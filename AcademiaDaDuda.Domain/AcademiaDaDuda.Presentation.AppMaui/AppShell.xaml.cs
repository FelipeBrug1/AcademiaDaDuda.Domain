// Felipe Antonio Brüggemann

using AcademiaDaDuda.Presentation.AppMaui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDaDuda.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(LogradouroPage), typeof(LogradouroPage));

        var dashboard = new FlyoutItem
        {
            Title = "Dashboard",
            Route = "dashboard",
            FlyoutIcon = CreateMenuIcon("\ue871")
        };
        dashboard.Items.Add(new ShellContent
        {
            Title = "Dashboard",
            Route = "inicio",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<DashboardPage>())
        });

        var logradouros = new FlyoutItem
        {
            Title = "Logradouros",
            Route = "logradouros",
            FlyoutIcon = CreateMenuIcon("\ue7f1")
        };
        logradouros.Items.Add(new ShellContent
        {
            Title = "Logradouros",
            Route = "lista-logradouros",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<LogradouroListPage>())
        });

        var settings = new FlyoutItem
        {
            Title = "Configurações",
            Route = "configuracoes",
            FlyoutIcon = CreateMenuIcon("\ue8b8")
        };
        settings.Items.Add(new ShellContent
        {
            Title = "Configurações",
            Route = "preferencias",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<SettingsPage>())
        });

        Items.Add(dashboard);
        Items.Add(logradouros);
        Items.Add(settings);
    }

    private static FontImageSource CreateMenuIcon(string glyph) => new()
    {
        Glyph = glyph,
        FontFamily = "MaterialIcons",
        Size = 22,
        Color = Color.FromArgb("#6C4CE5")
    };
}
