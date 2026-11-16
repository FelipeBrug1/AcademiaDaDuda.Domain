using System.Globalization;
using Microsoft.Maui.Controls;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

public sealed class LocalizedFormatConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not string format)
            return string.Empty;

        var value = ReferenceEquals(values[1], BindableProperty.UnsetValue) ? null : values[1];
        try
        {
            return string.Format(LocalizationManager.Instance.CurrentCulture, format, value);
        }
        catch (FormatException)
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A formatação localizada é somente de ida.");
}
