using System.Text.RegularExpressions;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class ColaboradorPage
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private void OnEmailUnfocused(object? sender, FocusEventArgs e)
    {
        var email = EmailEntry?.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.IsVisible = false;
            return;
        }

        if (!EmailPattern.IsMatch(email))
        {
            EmailErrorLabel.Text = "Formato de e-mail inválido. Use nome@dominio.com.";
            EmailErrorLabel.IsVisible = true;
            EmailEntry?.Focus();
            return;
        }

        EmailErrorLabel.IsVisible = false;
    }
}
