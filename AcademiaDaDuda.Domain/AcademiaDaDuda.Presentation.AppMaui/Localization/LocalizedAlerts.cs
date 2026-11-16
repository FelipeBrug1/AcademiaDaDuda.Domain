using Microsoft.Maui.Controls;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

public static class LocalizedAlerts
{
    public static Task DisplayAlertAsync(string title, string message, string cancel) =>
        Shell.Current.DisplayAlertAsync(
            LocalizationManager.Instance.Translate(title),
            LocalizationManager.Instance.TranslateMessage(message),
            LocalizationManager.Instance.Translate(cancel));

    public static async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        var result = await Shell.Current.DisplayAlertAsync(
            LocalizationManager.Instance.Translate(title),
            LocalizationManager.Instance.TranslateMessage(message),
            LocalizationManager.Instance.Translate(accept),
            LocalizationManager.Instance.Translate(cancel));
        return result;
    }

    public static async Task<string> DisplayActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons)
    {
        var displayed = buttons.Select(LocalizationManager.Instance.Translate).ToArray();
        var selected = await Shell.Current.DisplayActionSheetAsync(
            LocalizationManager.Instance.Translate(title),
            LocalizationManager.Instance.Translate(cancel),
            destruction is null ? null : LocalizationManager.Instance.Translate(destruction),
            displayed);

        var selectedIndex = Array.IndexOf(displayed, selected);
        return selectedIndex >= 0 ? buttons[selectedIndex] : selected;
    }
}
