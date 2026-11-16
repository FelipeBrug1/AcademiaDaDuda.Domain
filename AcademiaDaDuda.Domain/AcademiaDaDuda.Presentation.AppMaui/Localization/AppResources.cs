using System.Globalization;
using System.Resources;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

/// <summary>Strongly typed access point for the application's .resx resources.</summary>
internal static class AppResources
{
    private static readonly ResourceManager Manager = new(
        "AcademiaDaDuda.Presentation.AppMaui.Resources.Strings.AppResources",
        typeof(AppResources).Assembly);

    public static CultureInfo? Culture { get; set; }

    public static string? GetString(string key) => Manager.GetString(key, Culture);

    public static ResourceSet? GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents) =>
        Manager.GetResourceSet(culture, createIfNotExists, tryParents);
}
