using System.Globalization;

namespace AcademiaDaDuda.Presentation.AppMaui.Converters;

public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : DateTime.Today;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime date ? DateOnly.FromDateTime(date) : DateOnly.FromDateTime(DateTime.Today);
}
