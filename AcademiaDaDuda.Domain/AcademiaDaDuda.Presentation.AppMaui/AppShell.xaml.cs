// Felipe Antonio Brüggemann

using AcademiaDaDuda.Presentation.AppMaui.Views;
using Microsoft.Extensions.DependencyInjection;
using AcademiaDaDuda.Presentation.AppMaui.Localization;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui;

public partial class AppShell : Shell, IRecipient<CulturePreferencesUpdatedMessage>
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(LogradouroPage), typeof(LogradouroPage));
        Routing.RegisterRoute(nameof(AlunoPage), typeof(AlunoPage));
        Routing.RegisterRoute(nameof(ColaboradorPage), typeof(ColaboradorPage));
        Routing.RegisterRoute(nameof(MatriculaPage), typeof(MatriculaPage));
        Routing.RegisterRoute(nameof(AcessoPage), typeof(AcessoPage));

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

        var alunos = new FlyoutItem
        {
            Title = "Alunos",
            Route = "alunos",
            FlyoutIcon = CreateMenuIcon("\ue80c")
        };
        alunos.Items.Add(new ShellContent
        {
            Title = "Alunos",
            Route = "lista-alunos",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<AlunoListPage>())
        });

        var colaboradores = new FlyoutItem
        {
            Title = "Colaboradores",
            Route = "colaboradores",
            FlyoutIcon = CreateMenuIcon("\uea67")
        };
        colaboradores.Items.Add(new ShellContent
        {
            Title = "Colaboradores",
            Route = "lista-colaboradores",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<ColaboradorListPage>())
        });

        var matriculas = new FlyoutItem
        {
            Title = "Matrículas",
            Route = "matriculas",
            FlyoutIcon = CreateMenuIcon("\ue8f9")
        };
        matriculas.Items.Add(new ShellContent
        {
            Title = "Matrículas",
            Route = "lista-matriculas",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<MatriculaListPage>())
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

        var acesso = new FlyoutItem
        {
            Title = "Controle de Acesso",
            Route = "acesso",
            FlyoutIcon = CreateMenuIcon("\ue8f9")
        };
        acesso.Items.Add(new ShellContent
        {
            Title = "Controle de Acesso",
            Route = "controle-acesso",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<AcessoPage>())
        });

        Items.Add(dashboard);
        Items.Add(logradouros);
        Items.Add(alunos);
        Items.Add(colaboradores);
        Items.Add(matriculas);
        Items.Add(settings);
        Items.Add(acesso);
        WeakReferenceMessenger.Default.Register(this);
        ApplyLocalizedTitles();
    }

    public void Receive(CulturePreferencesUpdatedMessage message) => ApplyLocalizedTitles();

    private void ApplyLocalizedTitles()
    {
        var localizer = LocalizationManager.Instance;
        SetItemTitle(0, localizer.Translate("MenuDashboard"));
        SetItemTitle(1, localizer.Translate("MenuLogradouros"));
        SetItemTitle(2, localizer.Translate("MenuAlunos"));
        SetItemTitle(3, localizer.Translate("MenuColaboradores"));
        SetItemTitle(4, localizer.Translate("Matrículas"));
        SetItemTitle(5, localizer.Translate("Configurações"));
        SetItemTitle(6, localizer.Translate("MenuAcesso"));
    }

    private void SetItemTitle(int index, string title)
    {
        if (Items.Count <= index || Items[index] is not FlyoutItem item)
            return;

        item.Title = title;
        foreach (var shellContent in item.Items)
            shellContent.Title = title;
    }

    private static FontImageSource CreateMenuIcon(string glyph) => new()
    {
        Glyph = glyph,
        FontFamily = "MaterialIcons",
        Size = 22,
        Color = Color.FromArgb("#6C4CE5")
    };
}
