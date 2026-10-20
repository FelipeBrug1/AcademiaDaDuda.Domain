// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DependencyInjection;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell _appShell;
    private readonly RepositoryConfig _repositoryConfig;

    public App(AppShell appShell, RepositoryConfig repositoryConfig)
    {
        InitializeComponent();
        _appShell = appShell;
        _repositoryConfig = repositoryConfig;

        var savedTheme = Preferences.Get(DatabaseSettingsStartup.ThemeKey, "System") switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        ApplyTheme(savedTheme);
        RequestedThemeChanged += (_, eventArgs) => ApplyPalette(eventArgs.RequestedTheme);

        WeakReferenceMessenger.Default.Register<App, TemaPreferenciasAtualizadasMessage>(this,
            static (recipient, message) => recipient.ApplyTheme(message.Value));
        WeakReferenceMessenger.Default.Register<App, BancoPreferenciasAtualizadasMessage>(this,
            static (recipient, message) =>
            {
                recipient._repositoryConfig.DatabaseType = message.Value.DatabaseType;
                recipient._repositoryConfig.ConnectionString = message.Value.ConnectionString;
            });

    }

    private void ApplyTheme(AppTheme theme)
    {
        UserAppTheme = theme;
        ApplyPalette(theme == AppTheme.Unspecified ? RequestedTheme : theme);
    }

    private void ApplyPalette(AppTheme theme)
    {
        var dark = theme == AppTheme.Dark;
        Resources["PageBackgroundColor"] = Color.FromArgb(dark ? "#12111A" : "#F5F6FA");
        Resources["SurfaceColor"] = Color.FromArgb(dark ? "#211F2B" : "#FFFFFF");
        Resources["SurfaceMutedColor"] = Color.FromArgb(dark ? "#302D3C" : "#E8E6EF");
        Resources["PrimaryTextColor"] = Color.FromArgb(dark ? "#F4F1FF" : "#25233A");
        Resources["SecondaryTextColor"] = Color.FromArgb(dark ? "#B6B1C8" : "#77758B");
        Resources["BorderColor"] = Color.FromArgb(dark ? "#3B374A" : "#E8E5F0");
        Resources["BrandColor"] = Color.FromArgb(dark ? "#A78BFA" : "#6C4CE5");
        Resources["BrandDarkColor"] = Color.FromArgb(dark ? "#C4B5FD" : "#5135B4");
        Resources["BrandSoftColor"] = Color.FromArgb(dark ? "#3A3156" : "#E9E3FF");
        Resources["BrandAccentColor"] = Color.FromArgb(dark ? "#9B83F4" : "#8A6CF0");
        Resources["DashboardFeatureBackgroundColor"] = Color.FromArgb(dark ? "#29213E" : "#34245F");
        Resources["DashboardFeatureSecondaryColor"] = Color.FromArgb(dark ? "#D5C9FF" : "#DDD5F5");
        Resources["DashboardFeatureMutedColor"] = Color.FromArgb(dark ? "#B8A9EC" : "#C8BCEB");
        Resources["DangerColor"] = Color.FromArgb(dark ? "#FF9AB6" : "#A62E57");
        Resources["DangerSoftColor"] = Color.FromArgb(dark ? "#492938" : "#FCE8EE");
        Resources["StudentStatColor"] = Color.FromArgb(dark ? "#65D9D4" : "#168F91");
        Resources["EmployeeStatColor"] = Color.FromArgb(dark ? "#FFA1C2" : "#D65D85");
        Resources["EnrollmentStatColor"] = Color.FromArgb(dark ? "#FFC564" : "#CE8C27");
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_appShell);
}
