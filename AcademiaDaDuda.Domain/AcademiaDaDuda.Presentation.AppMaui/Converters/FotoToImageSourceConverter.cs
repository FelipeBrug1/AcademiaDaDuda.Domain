using AcademiaDaDuda.Application.DTOs;
using System.Globalization;

namespace AcademiaDaDuda.Presentation.AppMaui.Converters;

public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var bytes = value switch
        {
            ArquivoDto arquivo => arquivo.Conteudo,
            byte[] conteudo => conteudo,
            _ => null
        };

        if (bytes is { Length: > 0 })
            return ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));

        return new FontImageSource
        {
            FontFamily = "MaterialIcons",
            Glyph = "\ue7fd",
            Size = 48,
            Color = Color.FromArgb("#8B91A7")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A conversão de imagem é somente de ida.");
}
