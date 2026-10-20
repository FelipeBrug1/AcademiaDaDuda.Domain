using AcademiaDaDuda.Application.Enums;
using System.Globalization;

namespace AcademiaDaDuda.Presentation.AppMaui.Converters;

public sealed class EnumDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Enum enumValue ? enumValue.GetDisplayName() : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A conversão de exibição do enum é somente de ida.");
}
