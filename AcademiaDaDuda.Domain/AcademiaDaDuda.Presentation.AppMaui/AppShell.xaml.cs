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

        var dashboard = new FlyoutItem { Title = "Dashboard", Route = "dashboard" };
        dashboard.Items.Add(new ShellContent
        {
            Title = "Dashboard",
            Route = "inicio",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<DashboardPage>())
        });

        var logradouros = new FlyoutItem { Title = "Logradouros", Route = "logradouros" };
        logradouros.Items.Add(new ShellContent
        {
            Title = "Logradouros",
            Route = "lista-logradouros",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<LogradouroListPage>())
        });

        Items.Add(dashboard);
        Items.Add(logradouros);
    }
}
